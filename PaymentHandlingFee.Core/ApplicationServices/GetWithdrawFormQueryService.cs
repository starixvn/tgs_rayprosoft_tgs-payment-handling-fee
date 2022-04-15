using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.HandlingFee.ApplicationServices;

internal class GetWithdrawFormQueryService : IGetService<GetWithdrawFormQuery, WithdrawFormInfo?>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public GetWithdrawFormQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<WithdrawFormInfo?> GetAsync(GetWithdrawFormQuery query, CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetWithdrawFormAsync(query.FormId, cancellationToken);
}
