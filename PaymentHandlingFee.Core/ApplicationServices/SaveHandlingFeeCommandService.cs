using Tgs.ServiceUtility;

namespace Tgs.Payment.HandlingFee.ApplicationServices;

internal class SaveHandlingFeeCommandService : ICommandService<SaveHandlingFeeCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public SaveHandlingFeeCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask ExecuteAsync(SaveHandlingFeeCommand command, CancellationToken cancellationToken = default)
	{
		var createInfo = new PaymentHandlingFeeCreateInfo(
			command.FormId,
			command.BrandId,
			command.Type,
			command.Amount,
			command.Fee,
			command.UserType,
			command.UserId,
			command.CreatedOn,
			command.ApprovedOn,
			command.Remark);

		await _paymentRepository
			.AddPaymentHandlingFeeRecordAsync(createInfo, cancellationToken)
			.ConfigureAwait(false);
	}
}
