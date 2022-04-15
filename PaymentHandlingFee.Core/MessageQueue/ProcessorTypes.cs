namespace Tgs.Payment.HandlingFee.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type DepositStatusChangeHandlerType = typeof(DepositStatusChangeHandler);
	public static readonly Type WithdrawStatusChangeHandlerType = typeof(WithdrawStatusChangeHandler);
}
