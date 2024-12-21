using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Personal.WPFClient.Helper;

public class ImageSourceToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        var imgs = value as ImageSource;
        return imgs; //?.ToIcon();
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        return null;
    }
}  

