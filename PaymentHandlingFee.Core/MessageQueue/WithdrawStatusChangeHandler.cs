using System.Text.Json.Nodes;
using Json.More;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.HandlingFee.ApplicationServices;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.HandlingFee.MessageQueue
{
    internal class WithdrawStatusChangeHandler : IMessageHandler
    {
        private readonly IGetService<GetWithdrawFormQuery, WithdrawFormInfo?> _getWithdrawFormService;
        private readonly IGetService<GetBrandInfoQuery, BrandInfo?> _getBrandInfoService;
        private readonly ICommandService<SaveHandlingFeeCommand> _saveHandlingFeeService;

        public WithdrawStatusChangeHandler(
            IGetService<GetWithdrawFormQuery, WithdrawFormInfo?> getWithdrawFormService,
            IGetService<GetBrandInfoQuery, BrandInfo?> getBrandInfoService,
            ICommandService<SaveHandlingFeeCommand> saveHandlingFeeService)
        {
            _getWithdrawFormService = getWithdrawFormService ?? throw new ArgumentNullException(nameof(getWithdrawFormService));
            _getBrandInfoService = getBrandInfoService ?? throw new ArgumentNullException(nameof(getBrandInfoService));
            _saveHandlingFeeService = saveHandlingFeeService ?? throw new ArgumentNullException(nameof(saveHandlingFeeService));
        }

        public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var formEvent = FormEventEx.Parser.ParseFrom(data.Span);
            var previousStatus = Enum.Parse<PaymentFlowState>(formEvent.Status);

            if (previousStatus != PaymentFlowState.WaitApprove)
                return;

            var withdrawForm = await _getWithdrawFormService
                .GetAsync(new GetWithdrawFormQuery(formEvent.FormId), cancellationToken)
                .ConfigureAwait(false);

            if (withdrawForm is null
                || withdrawForm.Status is not PaymentFlowState.Approved
                || withdrawForm.UserType is not UserType.Player or UserType.Agent)
                return;

            var remark = new JsonObject(new JsonNodeOptions { PropertyNameCaseInsensitive = true });
            remark.Add(
                "form",
                JsonObject.Create(withdrawForm.ToJsonDocument(PaymentSerializeUtility.SerializerOptions).RootElement));

            var brandInfo = await _getBrandInfoService
                .GetAsync(new GetBrandInfoQuery(withdrawForm.BrandId), cancellationToken)
                .ConfigureAwait(false);

            var withdrawAmount = withdrawForm.ActualAmount ?? withdrawForm.Amount;
            var withdrawFee = brandInfo!.WithdrawHandlingFeeRateTenPercentile > 0M
                ? withdrawAmount * brandInfo!.WithdrawHandlingFeeRateTenPercentile * 0.001M
                : 0M;

            var saveCommand = new SaveHandlingFeeCommand(
                withdrawForm.Id,
                withdrawForm.BrandId,
                PaymentType.Withdraw,
                withdrawAmount,
                withdrawFee,
                withdrawForm.UserType,
                withdrawForm.UserId!,
                withdrawForm.CreatedOn.UtcDateTime,
                withdrawForm.ReviewedOn!.Value.UtcDateTime,
                remark.ToJsonDocument(PaymentSerializeUtility.SerializerOptions));

            await _saveHandlingFeeService
                .ExecuteAsync(saveCommand, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
