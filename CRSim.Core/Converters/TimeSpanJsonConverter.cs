using System.Text.Json.Serialization;
using System.Text.Json;

namespace CRSim.Core.Converters
{
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan?>
    {
        private const string TimeFormat = @"hh\:mm\:ss";


        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string timeString = reader.GetString();
                if (string.IsNullOrEmpty(timeString)) return null;
                bool isNegative = timeString.StartsWith('-');
                string absoluteValue = isNegative ? timeString[1..] : timeString;
                if (TimeSpan.TryParseExact(absoluteValue, TimeFormat, null, out TimeSpan result))
                {
                    return isNegative ? -result : result;
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                if (value.Value < TimeSpan.Zero)
                {
                    writer.WriteStringValue("-" + value.Value.Duration().ToString(TimeFormat));
                }
                else
                {
                    writer.WriteStringValue(value.Value.ToString(TimeFormat));
                }
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

}
