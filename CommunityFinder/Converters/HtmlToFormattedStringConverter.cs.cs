using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Converters
{
    /// <summary>
    /// 将 HTML 标签转换为 FormattedString 用于在 Label 中显示格式化文本
    /// </summary>
    public class HtmlToFormattedStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string html))
                return new FormattedString();

            // 移除 HTML 标签中的属性（如 <span style="...">）
            html = Regex.Replace(html, @"<\w+[^>]*>", m =>
            {
                var tag = m.Value;
                // 保留纯标签，例如 <br /> -> <br>, <p> -> <p>
                if (tag.Contains("span"))
                    return "";
                if (tag.Contains("br"))
                    return "\n";
                if (tag.Contains("p"))
                    return "";
                if (tag.Contains("li"))
                    return "• ";
                if (tag.Contains("ul"))
                    return "";
                return tag;
            });

            // 移除关闭标签
            html = Regex.Replace(html, @"</\w+>", m =>
            {
                var tag = m.Value;
                if (tag.Contains("p"))
                    return "\n";
                if (tag.Contains("li"))
                    return "\n";
                return "";
            });

            // 处理多个连续换行符
            html = Regex.Replace(html, @"\n\n+", "\n");

            // 创建 FormattedString
            var formattedString = new FormattedString();
            var spans = html.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var span in spans)
            {
                if (string.IsNullOrWhiteSpace(span))
                    continue;

                formattedString.Spans.Add(new Span
                {
                    Text = span.Trim() + "\n",
                    FontSize = 15,
                    LineHeight = 1.5
                });
            }

            return formattedString;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}