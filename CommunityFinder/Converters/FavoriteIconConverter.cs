using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Converters
{
    public class FavoriteIconConverter : IValueConverter
    {
        // Converts IsFavorited(bool) → ⭐ (yellow star) or ☆ (white star outline)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFav = value is bool b && b;
            return isFav ? "🌟" : "⭐︎";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
    