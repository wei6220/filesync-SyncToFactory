using DownloadCenterRsyncBaseLog;
using DownloadCenterRsyncDateTime;
using DownloadCenterRsyncSetting;
using DownloadCenterRsyncToStorSimpleLog;
using System;
using System.Diagnostics;

namespace DownloadCenterRsyncSchedule
{
    class RsyncSchedule : RsyncBaseLog
    {
        private static string downloadcenterRsyncLogPath;
        private static string downloadcenterRsyncLogFile;
        private int scheduleFinishTime,rsyncScheduleLogLength;
        public bool scheduleFinishStatus,scheduleStartStatus,scheduleErrorStatus;
        private string scheduleLogPath, scheduleLogFile, scheduleLogMessage  = "", logMessage;
        private string rsyncScheduleLogLine;

        public static string ReadRsyncScheduleLogPath()
        {
            downloadcenterRsyncLogPath = RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/DownloadCenterLog", "Path");
            
            if (!(downloadcenterRsyncLogPath.Substring(downloadcenterRsyncLogPath.Length - 1) == "/" || downloadcenterRsyncLogPath.Substring(downloadcenterRsyncLogPath.Length - 1) == "\\"))
            {
                downloadcenterRsyncLogPath += "\\";
            }
            return downloadcenterRsyncLogPath;
        }

        public string ReadScheduleLogXMLSetting()
        {
            string rsyncScheduleLogXMLPath = "";

            rsyncScheduleLogXMLPath = RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/DownloadCenterLog", "Path");

            if (!(rsyncScheduleLogXMLPath.Substring(rsyncScheduleLogXMLPath.Length - 1) == "/" || rsyncScheduleLogXMLPath.Substring(rsyncScheduleLogXMLPath.Length - 1) == "\\"))
            {
                rsyncScheduleLogXMLPath += "\\";
            }

            return rsyncScheduleLogXMLPath;
        }

        public void WriteRsyncScheduleLog()
        {
            if (rsyncReadLogFile)
            {
                logMessage = "[Download Center][Success]Read Download Center Log File";
            }
            else if (rsyncReadLogFileException)
            {
                logMessage =  "[Download Center][Exception]" + rsyncReadLogFileExceptionMessage;
            }
            

            if (logMessage != null)
            {
                RsyncDateTime.WriteLog(logMessage);
            }
        }

        public bool VerifyRsyncScheduleStatus()
        {
            int cmdRsyncExe = 0;
            double scheduleTime;
            string rsyncScheduleError;
            bool scheduleStaus = false;
            
            if (scheduleFinishStatus && scheduleStartStatus)
            {
                scheduleLogMessage = "[Download Center][Success]Check Last Rsync Schedule is Success Finish";
                scheduleStaus = true;
            }
            else if (DownloadCenterLog.SearchSubString(ref rsyncScheduleLogLine, "Check Last Rsync Schedule is Success Finish"))
            {
                scheduleLogMessage = "[Download Center][Success]Check Last Rsync Schedule is Success Finish";
                scheduleStaus = true;
            }
            else if (scheduleFinishStatus != true && scheduleStartStatus)
            {
                if (scheduleErrorStatus != true)
                {
                    rsyncScheduleError = rsyncScheduleLogLine.Split(new string[] { " || " }, StringSplitOptions.None)[0];

                    scheduleTime = RsyncDateTime.DifferentDateTime(rsyncScheduleError);

                    scheduleFinishTime = Convert.ToInt32(RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/Schedule", "CheckScheduleTime"));

                    if (scheduleTime / 60 >= scheduleFinishTime)
                    {
                        rsyncScheduleLogLength = -2;
                        scheduleLogMessage = "[Download Center][Error]Check Last Rsync Schedule is Fail";
                        scheduleStaus = true;
                    }
                    else
                    {
                        foreach (Process process in Process.GetProcessesByName("rsync"))
                        {
                            cmdRsyncExe++;
                        }
                        if (cmdRsyncExe >= 2)
                        {
                            cmdRsyncExe = 0;
                            rsyncScheduleLogLength = -3;
                            scheduleLogMessage = "";
                            scheduleStaus = true;
                        }
                        cmdRsyncExe = 0;
                    }
                }
            }
            return scheduleStaus;
        }


        public void RsyncScheduleLogStatus()
        {
            if (DownloadCenterLog.SearchSubString(ref rsyncScheduleLogLine, "Rsync Schedule Start"))
            {
                scheduleStartStatus = true;
            }
            else if (DownloadCenterLog.SearchSubString(ref rsyncScheduleLogLine, "Rsync Schedule Finish"))
            {
                scheduleFinishStatus = true;
            }
            else if (DownloadCenterLog.SearchSubString(ref rsyncScheduleLogLine, "Check Last Rsync Schedule is Fail"))
            {
                scheduleErrorStatus = true;
            }
        }


        public override void ReadNewRsyncLogRecord(int jjk)
        {
            int rsyncStartLogIndex;
            bool rsyncScheduleExeInterrupt;

            rsyncStartLogIndex = rsyncLogFile.Length -1;
            
            while (rsyncStartLogIndex > 0)
            {
                rsyncScheduleLogLine = rsyncLogFile[rsyncStartLogIndex];

                RsyncScheduleLogStatus();

                rsyncScheduleExeInterrupt = VerifyRsyncScheduleStatus();

                if(rsyncScheduleExeInterrupt == true)
                {
                    break;
                }
                
                rsyncStartLogIndex--;
            } 
        }

        public int ReadScheduleLog()
        {
            string rsyncScheduleLogLocation;

            RsyncSetting.SettingScheduleLog();
            scheduleLogPath = RsyncSetting.XmlSetting.rsyncScheduleLogPath;
            scheduleLogFile = RsyncSetting.XmlSetting.rsyncScheduleLogFile;

            if (!(scheduleLogPath.Substring(scheduleLogPath.Length - 1) == "/" || scheduleLogPath.Substring(scheduleLogPath.Length - 1) == "\\"))
            {
                scheduleLogPath += "\\";
            }

            rsyncScheduleLogLocation = scheduleLogPath + RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearMonthDate) + "_" + scheduleLogFile;

            ReadLogFile(rsyncScheduleLogLocation, false);
            WriteRsyncScheduleLog();
           
            return rsyncScheduleLogLength;
        }
    }
}
