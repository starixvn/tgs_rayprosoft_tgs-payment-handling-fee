using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.HandlingFee.ApplicationServices;

internal class GetDepositFormQueryService : IGetService<GetDepositFormQuery, DepositFormInfo?>
{
	private readonly IPaymentServiceClient _paymentServiceClient;

	public GetDepositFormQueryService(
		IPaymentServiceClient paymentServiceClient)
	{
		_paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
	}

	public ValueTask<DepositFormInfo?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
		=> _paymentServiceClient.GetDepositFormAsync(query.FormId, cancellationToken);
}
