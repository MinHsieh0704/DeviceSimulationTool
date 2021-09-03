using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeviceSimulationTool.Converts
{
    public class BooleanToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Hidden;
            }

            try
            {
                bool flag = System.Convert.ToBoolean(value);
                if (!flag)
                {
                    return Visibility.Hidden;
                }

                return Visibility.Visible;
            }
            catch (Exception ex)
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
