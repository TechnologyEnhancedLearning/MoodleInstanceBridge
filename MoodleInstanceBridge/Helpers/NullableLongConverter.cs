using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoodleInstanceBridge.Helpers
{
    public class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:
                    return reader.GetInt64();

                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (string.IsNullOrWhiteSpace(str))
                        return null;

                    if (long.TryParse(str, out var value))
                        return value;

                    throw new JsonException($"Invalid long value: {str}");

                default:
                    throw new JsonException($"Unexpected token parsing long. Token: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
