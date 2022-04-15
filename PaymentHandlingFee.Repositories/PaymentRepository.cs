using Tgs.Payment.HandlingFee.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace Tgs.Payment.HandlingFee.Repositories;

internal class PaymentRepository : IPaymentRepository
{
    private readonly TgsContext _tgsContext;
    private readonly ISerialNumberGenerator _serialNumberGenerator;

    public PaymentRepository(TgsContext tgsContext, ISerialNumberGenerator serialNumberGenerator)
    {
        _tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
    }

    public async ValueTask AddPaymentHandlingFeeRecordAsync(PaymentHandlingFeeCreateInfo feeCreateInfo, CancellationToken cancellationToken)
    {
        var record = new PaymentHandlingFee
        {
            Id = await _serialNumberGenerator.GenerateAsync().ConfigureAwait(false),
            Fee = feeCreateInfo.Fee,
            Amount = feeCreateInfo.Amount,
            ApprovedOn = feeCreateInfo.ApprovedOn,
            BrandId = feeCreateInfo.BrandId,
            CreatedOn = feeCreateInfo.CreatedOn,
            FormId = feeCreateInfo.FormId,
            Remark = feeCreateInfo.Remark?.ToJsonString(),
            Type = feeCreateInfo.Type.ToString(),
            UserId = feeCreateInfo.UserId,
            UserType = (int)feeCreateInfo.UserType
        };

        await _tgsContext.PaymentHandlingFees
            .AddAsync(record, cancellationToken)
            .ConfigureAwait(false);

        await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
