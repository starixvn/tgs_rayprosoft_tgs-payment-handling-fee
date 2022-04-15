namespace Tgs.Payment.HandlingFee;

public interface IPaymentRepository
{
	ValueTask AddPaymentHandlingFeeRecordAsync(PaymentHandlingFeeCreateInfo feeCreateInfo, CancellationToken cancellationToken);
}
