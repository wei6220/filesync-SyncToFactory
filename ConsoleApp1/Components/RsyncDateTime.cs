using System;

namespace DownloadCenter
{
    class RsyncDateTime 
    {
        public enum TimeFormatType
        {
            YearMonthDateTimeChange,
            YearMonthDateTime,
            YearMonthDate,
            YearSMonthSDate,
            YearMonth,
            YearSMonthSDateTimeChange
        };

        public static string GetFormatType(TimeFormatType timeType)
        {
            string dataTime = "";

            if (timeType == TimeFormatType.YearMonthDateTime)
            {
                dataTime = "yyyy/MM/dd H:mm:ss";
            }
            else if (timeType == TimeFormatType.YearMonthDate)
            {
                dataTime = "yyyyMMdd";
            }
            else if (timeType == TimeFormatType.YearSMonthSDate)
            {
                dataTime = "yyyy/MM/dd";
            }
            else if (timeType == TimeFormatType.YearMonthDateTimeChange)
            {
                dataTime = "yyyyMMdd tt H:mm:ss";
            }
            else if (timeType == TimeFormatType.YearSMonthSDateTimeChange)
            {
                dataTime = "yyyy/MM/dd tt H:mm:ss";
            }
            else
            {
                dataTime = "yyyyMM ";
            }

            return dataTime;
        }

        public static string GetNow(TimeFormatType formatType = TimeFormatType.YearMonthDateTime)
        {
            return DateTime.Now.ToString(GetFormatType(formatType));
        }

        public static double GetDiffSeconds(string oldTime)
        {
            DateTime sDate = Convert.ToDateTime(oldTime);
            DateTime eDate = DateTime.Now;
            TimeSpan dataTime = eDate - sDate;
            return dataTime.TotalSeconds;
        }
    }
}
