using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UIClient.Infrastructure.Converters
{
    public class ConvertByteArrayToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] array = (byte[])value;
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = (string)value;
            hex = hex.Replace(" ", "");
            byte[] ret;
                ret = Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
            return ret;
        }
    }
}
