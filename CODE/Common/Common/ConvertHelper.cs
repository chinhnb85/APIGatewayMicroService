using System;

namespace Common
{
    public class ConvertHelper
    {
        public static DateTime ConvertToDateTime(object value)
        {
            if (null == value || !DateTime.TryParse(value.ToString(), out DateTime returnValue))
            {
                returnValue = DateTime.MinValue;
            }
            return returnValue;
        }

        public static int ConvertToInt(object value)
        {
            int returnValue;
            if (null == value || !int.TryParse(value.ToString(), out returnValue))
            {
                returnValue = 0;
            }
            return returnValue;
        }
    }
}
