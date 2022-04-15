using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment.HandlingFee;

internal static class PaymentSerializeUtility
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNameCaseInsensitive = false
    };
}
