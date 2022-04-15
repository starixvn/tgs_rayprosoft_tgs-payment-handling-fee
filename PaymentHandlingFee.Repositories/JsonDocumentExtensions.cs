using System.Text;
using System.Text.Json;

namespace Tgs.Payment.HandlingFee.Repositories;

internal static class JsonDocumentExtensions
{
    public static string ToJsonString(this JsonDocument jdoc)
    {
        using (var stream = new MemoryStream())
        {
            Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            jdoc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
