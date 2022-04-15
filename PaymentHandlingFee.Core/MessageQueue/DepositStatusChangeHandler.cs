using System.Text.Json.Nodes;
using Json.More;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.HandlingFee.ApplicationServices;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.HandlingFee.MessageQueue;

internal class DepositStatusChangeHandler : IMessageHandler
{
	private readonly IGetService<GetDepositFormQuery, DepositFormInfo?> _getDepositFormService;
	private readonly IGetService<GetBrandInfoQuery, BrandInfo?> _getBrandInfoService;
	private readonly ICommandService<SaveHandlingFeeCommand> _saveHandlingFeeService;

	public DepositStatusChangeHandler(
		IGetService<GetDepositFormQuery, DepositFormInfo?> getDepositFormService,
		IGetService<GetBrandInfoQuery, BrandInfo?> getBrandInfoService,
		ICommandService<SaveHandlingFeeCommand> saveHandlingFeeService)
	{
		_getDepositFormService = getDepositFormService ?? throw new ArgumentNullException(nameof(getDepositFormService));
		_getBrandInfoService = getBrandInfoService ?? throw new ArgumentNullException(nameof(getBrandInfoService));
		_saveHandlingFeeService = saveHandlingFeeService ?? throw new ArgumentNullException(nameof(saveHandlingFeeService));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var formEvent = FormEventEx.Parser.ParseFrom(data.Span);
		var previousStatus = Enum.Parse<PaymentFlowState>(formEvent.Status);

		if (previousStatus != PaymentFlowState.WaitApprove)
			return;

		var depositForm = await _getDepositFormService
			.GetAsync(new GetDepositFormQuery(formEvent.FormId), cancellationToken)
			.ConfigureAwait(false);

		if (depositForm is null || depositForm.Status != PaymentFlowState.Approved)
			return;

		var remark = new JsonObject(new JsonNodeOptions { PropertyNameCaseInsensitive = true });
		remark.Add(
			"form",
			JsonObject.Create(depositForm.ToJsonDocument(PaymentSerializeUtility.SerializerOptions).RootElement));

		var brandInfo = await _getBrandInfoService
			.GetAsync(new GetBrandInfoQuery(depositForm.BrandId), cancellationToken)
			.ConfigureAwait(false);

		var depositAmount = depositForm.ActualAmount ?? depositForm.Amount;
		var depositFee = brandInfo!.DepositHandlingFeeRateTenPercentile > 0M
			? depositAmount * brandInfo!.DepositHandlingFeeRateTenPercentile * 0.001M
			: 0M;

		var saveCommand = new SaveHandlingFeeCommand(
			depositForm.Id,
			depositForm.BrandId,
			PaymentType.Deposit,
			depositAmount,
			depositFee,
			UserType.Player,
			depositForm.PlayerCode,
			depositForm.CreatedOn.UtcDateTime,
			depositForm.ReviewedOn!.Value.UtcDateTime,
			remark.ToJsonDocument(PaymentSerializeUtility.SerializerOptions));

		await _saveHandlingFeeService
			.ExecuteAsync(saveCommand, cancellationToken)
			.ConfigureAwait(false);
	}
}
