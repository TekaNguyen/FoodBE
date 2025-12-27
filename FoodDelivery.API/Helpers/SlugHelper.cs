using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace FoodDelivery.API.Helpers;

// ⚠️ QUAN TRỌNG NHẤT: Phải có chữ 'partial' ở đây!
public static partial class SlugHelper
{
    // Regex cho ký tự đặc biệt
    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharsRegex();

    // Regex cho khoảng trắng thừa
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleSpacesRegex();

    public static string Generate(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase)) return string.Empty;

        string str = phrase.ToLowerInvariant();

        str = RemoveDiacritics(str);

        // Gọi hàm (có dấu ngoặc tròn)
        str = InvalidCharsRegex().Replace(str, "");

        str = MultipleSpacesRegex().Replace(str, "-").Trim('-');

        return str;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}