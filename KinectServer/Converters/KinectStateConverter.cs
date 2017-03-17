using KinectServer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KinectServer.Converters
{
    public class KinectStateConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            KinectState state = (KinectState)value;
            switch (state)
            {
                case KinectState.NOT_READY:
                    return "Not Ready";
                case KinectState.READY:
                    return "Ready";
                case KinectState.STARTED:
                    return "Started";
                case KinectState.STOPPED:
                    return "Stopped";
                default:
                    return "Unknown State";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
