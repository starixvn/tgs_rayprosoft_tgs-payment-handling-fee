using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.HandlingFee.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
			fluentOptions.UseMessagePack();
		});
	});

builder.Services
	.AddPaymentHandlingFee((options, _) =>
	{
		options.PaymentServiceBaseUrl = builder.Configuration.GetValue<Uri>("HandlingFee:PaymentServiceBaseUrl");
	})
	.AddPaymentHandlingFeeRepositories(dbOptions => dbOptions
		.UseNpgsql("name=ConnectionStrings:Tgs"))
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) =>
		{
			options.Url = builder.Configuration.GetConnectionString("Nats");
		})
		.AddHandler(
			ProcessorTypes.DepositStatusChangeHandlerType,
			"tgs.payment.deposit.status_ex",
			"tgs.payment.handling_fee.deposit")
		.AddHandler(
			ProcessorTypes.WithdrawStatusChangeHandlerType,
			"tgs.payment.withdraw.status_ex",
			"tgs.payment.handling_fee.withdraw"));

builder.Services
	.AddHealthChecks();

builder.Services
	.AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
		sp => new ConfigureNamedOptions<OtlpExporterOptions>(
			Microsoft.Extensions.Options.Options.DefaultName,
			exporterOptions =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var nodeIp = config.GetValue<string>("NODE_IP");

				if (!string.IsNullOrWhiteSpace(nodeIp))
					exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

				config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
			}))
	.AddOpenTelemetryTracing(
		tracingBuilder => tracingBuilder
			.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddEnvironmentVariableDetector())
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddAspNetCoreInstrumentation()
			.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
			.AddHttpClientInstrumentation()
			.AddNpgsql()
			.AddProfileViewExporter()
			.AddOtlpExporter())
	.Configure<AspNetCoreInstrumentationOptions>(
		options => options.Filter = httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
	.AddProfileViewer();

var app = builder.Build();

app.MapProfileViewer();
app.MapHealthChecks("/healthz");

app.Run();
