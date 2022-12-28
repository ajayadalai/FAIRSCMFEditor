using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FAIRSCMFEditor.View
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class LocalDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                var dtValue = System.Convert.ToDateTime(value).ToLocalTime();
                return dtValue;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                var dtValue = System.Convert.ToDateTime(value).ToUniversalTime();
                return dtValue;
            }
            return null;
        }
    }
}
