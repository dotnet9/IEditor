using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IEditor.App.Services;

public static class FilenameTemplateFormatter
{
    public static string Format(string template, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            template = "{原名}";
        }

        var result = template;
        foreach (var token in tokens.OrderByDescending(item => item.Key.Length))
        {
            result = result.Replace("{" + token.Key + "}", token.Value ?? string.Empty, StringComparison.Ordinal);
        }

        return Sanitize(result);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        Span<char> buffer = value.Length <= 256 ? stackalloc char[value.Length] : new char[value.Length];
        var index = 0;
        foreach (var ch in value.Trim())
        {
            buffer[index++] = invalid.Contains(ch) ? '_' : ch;
        }

        var sanitized = new string(buffer[..index]);
        while (sanitized.Contains("  ", StringComparison.Ordinal))
        {
            sanitized = sanitized.Replace("  ", " ", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "未命名" : sanitized;
    }
}
