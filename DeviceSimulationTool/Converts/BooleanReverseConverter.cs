using System;
using System.Globalization;
using System.Windows.Data;

namespace DeviceSimulationTool.Converts
{
    public class BooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                bool flag = System.Convert.ToBoolean(value);
                if (!flag)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
