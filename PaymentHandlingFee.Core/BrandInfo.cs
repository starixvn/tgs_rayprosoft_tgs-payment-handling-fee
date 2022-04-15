namespace Tgs.Payment.HandlingFee;

public record BrandInfo(
    Guid Id,
    decimal DepositHandlingFeeRateTenPercentile,
    decimal WithdrawHandlingFeeRateTenPercentile);
