using System;

namespace DownloadCenterRsyncBaseTime
{
    abstract class RsyncBaseTime
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

         public static string GetTimeFormatType(TimeFormatType timeType)
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

         public static string GetTimeNow(TimeFormatType getTimeFormatType = TimeFormatType.YearMonthDateTime)
         {
            string rsyncCreateTime = "",rsyncDataTimeType = "";
            DateTime timeStart;

            rsyncDataTimeType = GetTimeFormatType(getTimeFormatType);
            timeStart = DateTime.Now;
            rsyncCreateTime = ToStringDataTime(timeStart, rsyncDataTimeType);

            return rsyncCreateTime;
         }

         public static string ToStringDataTime(DateTime getTimeStart, string getTimeStartFormat)
         {
            string rsyncDataTime = "";

            rsyncDataTime = getTimeStart.ToString(getTimeStartFormat);

            return rsyncDataTime;
         }
    }
}
