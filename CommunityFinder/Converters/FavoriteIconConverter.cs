using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CommunityFinder.Converters
{
    /// <summary>
    /// 将收藏状态 bool 转换为安卓/iOS 可显示的星星图标
    /// </summary>
    public class FavoriteIconConverter : IValueConverter
    {
        /// <summary>
        /// 根据收藏状态返回星星字符（空心/实心）
        /// </summary>
        /// <param name="value">bool 类型，是否收藏</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>实心或空心星</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFavorited = value is bool b && b;

            // 安卓/iOS 都可以显示的 Unicode 星星
            return isFavorited ? "🌟" : "⭐"; // 实心或空心星
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
