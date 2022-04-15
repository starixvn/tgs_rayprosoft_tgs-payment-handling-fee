using System.Reflection;
using Tgs.Payment.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentHandlingFee(
		this IServiceCollection services,
		Action<PaymentServiceClientOptions, IServiceProvider> paymentServiceClientConfigure)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddPaymentServiceClient(paymentServiceClientConfigure);
}
