using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Converters
{
    public class LikeIconConverter : IValueConverter
    {
        // Converts IsLiked(bool) → Text emoji ❤️ (red heart) or 🤍 (white heart)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLiked = value is bool b && b;
            return isLiked ? "❤️" : "🤍";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
