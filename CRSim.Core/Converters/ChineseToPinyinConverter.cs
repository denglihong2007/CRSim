using Microsoft.International.Converters.PinYinConverter;
using System.Text;
using System.Text.Json;

namespace CRSim.Core.Converters
{
    public static class ChineseToPinyinConverter
    {
        private static readonly Dictionary<string, string> ChineseToPinyinMap = JsonDocument.Parse(
                    new StreamReader(typeof(ChineseToPinyinConverter).Assembly.GetManifestResourceStream("CRSim.Core.Assets.District.json")
                    ?? throw new InvalidOperationException())
                    .ReadToEnd())
                    .RootElement.EnumerateArray()
                    .ToDictionary(x => x.GetProperty("name").GetString()!, x => x.GetProperty("pinyin").GetString()!);
        public static string ConvertToPinyin(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            StringBuilder result = new();
            int i = 0;

            while (i < input.Length)
            {
                bool matched = false;

                // 1. 尝试在字典中匹配连续字符组
                for (int len = Math.Min(input.Length - i, 10); len >= 2; len--)
                {
                    string sub = input.Substring(i, len);
                    if (ChineseToPinyinMap.TryGetValue(sub, out var pinyin))
                    {
                        result.Append(pinyin);
                        i += len;
                        matched = true;
                        break;
                    }
                }

                // 2. 如果字典没匹配到，处理单个字符
                if (!matched)
                {
                    char c = input[i];
                    if (c == '南')
                    {
                        result.Append("nan");
                    }
                    else if (ChineseChar.IsValidChar(c))
                    {
                        // 使用 Microsoft PinYinConverter 获取第一个拼音
                        ChineseChar chineseChar = new(c);
                        if (chineseChar.Pinyins.Count > 0 && chineseChar.Pinyins[0] != null)
                        {
                            string rawPinyin = chineseChar.Pinyins[0].ToString();
                            string purePinyin = new string([.. rawPinyin.Where(ch => !char.IsDigit(ch))]).ToLower();
                            result.Append(purePinyin);
                        }
                    }
                    else
                    {
                        // 非汉字字符（数字、字母、标点）原样保留
                        result.Append(c);
                    }
                    i++;
                }
            }

            return result.ToString();
        }
    }
}
