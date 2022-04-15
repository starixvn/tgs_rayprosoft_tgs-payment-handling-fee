using Microsoft.EntityFrameworkCore;
using Tgs.Payment.HandlingFee;
using Tgs.Payment.HandlingFee.Repositories;
using Tgs.Payment.HandlingFee.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentHandlingFeeRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbContextOptionsAction)
		=> services
			.AddDbContext<TgsContext>(dbContextOptionsAction)
			.AddSerialNumberGenerator()
			.AddTransient<IPaymentRepository, PaymentRepository>();
}
