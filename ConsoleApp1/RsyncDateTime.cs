using DownloadCenterRsyncBaseTime;
using DownloadCenterRsyncSetting;
using System;

namespace DownloadCenterRsyncDateTime
{
    class RsyncDateTime : RsyncBaseTime
    {
        public static string rsyncScheduleWriteLogPath, rsyncScheduleWriteLogFile;

        public static double DifferentDateTime(string oldTime)
        {
            DateTime sDate = Convert.ToDateTime(oldTime);
            DateTime eDate = DateTime.Now;
            TimeSpan dataTime = eDate - sDate;
            return dataTime.TotalSeconds;
        }
      
        public static void WriteLog(string rsyncLog)
        {
            string rsyncWriteLogTimeNow = "",rsyncFileLogTimeNow = "";

            RsyncSetting.SettingScheduleLog();
            rsyncScheduleWriteLogPath = RsyncSetting.XmlSetting.rsyncScheduleLogPath;
            rsyncScheduleWriteLogFile = RsyncSetting.XmlSetting.rsyncScheduleLogFile;


            rsyncWriteLogTimeNow = GetTimeNow();
            rsyncFileLogTimeNow = GetTimeNow(TimeFormatType.YearMonthDate);
            
            CommonLibrary.LogHelper.Write(rsyncLog, rsyncFileLogTimeNow + "_" + rsyncScheduleWriteLogFile, rsyncScheduleWriteLogPath);
        }
    }
}
