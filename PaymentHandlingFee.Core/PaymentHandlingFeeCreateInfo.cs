using System.Text.Json;
using Tgs.Payment.Clients;

namespace Tgs.Payment.HandlingFee;

public record PaymentHandlingFeeCreateInfo(
	Guid FormId,
	Guid BrandId,
	PaymentType Type,
	decimal Amount,
	decimal Fee,
	UserType UserType,
	string UserId,
	DateTime CreatedOn,
	DateTime ApprovedOn,
	JsonDocument? Remark);
