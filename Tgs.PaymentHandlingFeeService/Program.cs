using Microsoft.EntityFrameworkCore;
using Tgs.Payment.HandlingFee.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
