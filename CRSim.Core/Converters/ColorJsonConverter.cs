using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace CRSim.Core.Converters
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 从十六进制字符串（如 "#FF00BE"）读取并转换回 Color
            string? hex = reader.GetString();
            return hex == null ? Color.Empty : ColorTranslator.FromHtml(hex);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            // 将 Color 转换为十六进制字符串格式写入 JSON
            if (value.IsEmpty) writer.WriteNullValue();
            else writer.WriteStringValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
        }
    }
}
