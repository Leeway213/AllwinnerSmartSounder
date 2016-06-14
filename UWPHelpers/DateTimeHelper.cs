using System;

namespace UWPHelpers
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 字符串转化为DateTime对象。
        /// 可转换的字符串类型：yyyy-mm-dd、yyyy-mm-dd hh:mm:ss、yyyy-mm-ddThh:mm、yyyy-mm-ddThh、yyyy-Wxx-WE
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime String2DateTime(string str) 
        {
            DateTime dt = new DateTime();

            if (DateTime.TryParse(str, out dt))
            {
                return dt;
            }
            else if (str.Contains("T"))
            {
                if (!str.Contains(":"))
                {
                    str += ":00";
                    if (DateTime.TryParse(str, out dt))
                    {
                        return dt;
                    }
                }
            }
            else
            {
                var s = str.Split('-');
                foreach (var c in s)
                {
                    int i;
                    if (int.TryParse(c, out i))
                    {
                        dt = dt.AddYears(i - 1);
                    }
                    else
                    {
                        if (c.Equals("WE"))
                        {
                            dt = dt.AddDays(9);
                        }
                        else if (int.TryParse(c.Substring(1), out i))
                        {
                            dt = dt.AddDays(7 * (i - 1));
                        }
                    }
                }
                return dt;
            }

            throw new Exception();
        }
    }
}
