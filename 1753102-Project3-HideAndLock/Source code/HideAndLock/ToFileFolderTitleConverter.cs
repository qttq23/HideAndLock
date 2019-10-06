using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HideAndLock
{
    class ToFileFolderTitleConverter : IMultiValueConverter
    {
        public static ToFileFolderTitleConverter Instance = new ToFileFolderTitleConverter();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";

            result = (string)values[0] + "(" + ((int)values[1]).ToString() + ")";

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
