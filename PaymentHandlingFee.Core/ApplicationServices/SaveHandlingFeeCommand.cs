using System.Text.Json;
using System.Text.Json.Nodes;
using Tgs.Payment.Clients;

namespace Tgs.Payment.HandlingFee.ApplicationServices;

public record SaveHandlingFeeCommand(
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
