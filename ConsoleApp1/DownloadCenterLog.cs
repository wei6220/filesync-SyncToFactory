using System;
using System.Collections.Generic;
using DownloadCenterRsyncDateTime;
using System.IO;
using DownloadCenterRsyncSetting;
using DownloadCenterRsyncBaseLog;

namespace DownloadCenterRsyncToStorSimpleLog
{
    class DownloadCenterLog : RsyncBaseLog
    {
        public int totalrsyncfile
        {
            get { return Counter; }
        }

        public int GetRsyncLogLength
        {
            get { return rsynLogLength; }
        }

        public static string GetRsyncHtmlLog
        {
            get { return RsyncHtmlLog; }
        }

        List<string> RsyncType = new List<string>
        {
            "cd+++++++++ send ",
            "*deleting   del. ",
            ">f+++++++++ send ",
            ">f.st...... send ",
            "rsync: link_stat ",
            "failed: File exists",
            " failed: No such host or network path",
            "rsync: change_dir#3",
        };

        private int Counter = 0;
        private static int rsynLogLength = 0;
        private string rsyncLogLine;
        private string[] RsyncLogDetail;
        private string RsyncCommandStart;
        private string RsyncFileLog;
        public  static string RsyncHtmlLog;
        private string DownloadCenterRsyncLog;
        private string rsyncLogFileName,rsyncLogPath,rsyncSourceFolder, rsyncTargetFolder;
        private string rsyncLogLocation,rsyncTargetHost,errorMessage;
        private bool rsyncLogErrorStatus;

        public bool ReadThreadsRsyncLog()
        {
            bool threadReadRsyncLogFlag;
            try
            {
                SettingReadRsyncLogFilePath();
                if (File.Exists(rsyncLogLocation))
                {
                    int threeadRsyncLogLength;
                    using (FileStream file = new FileStream(rsyncLogLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        string[] threadRsyncLogFile = File.ReadAllLines(rsyncLogLocation);
                        threeadRsyncLogLength = threadRsyncLogFile.Length;
                        file.Close();
                        
                    }
                }
                threadReadRsyncLogFlag = false;
            }
            catch
            {
                threadReadRsyncLogFlag = true;
            }
            return threadReadRsyncLogFlag;
        }

        public string GetRsynLogPath(string getfolder)
        {
            string[] listFolder;
            string exeCommandFolderParse = "";
            string folderPath = "";
            int i = 0;

            listFolder = getfolder.Split(new char[] { '\\', '/' });

            foreach (string foldername in listFolder)
            {
                if (i != 0)
                {
                    exeCommandFolderParse += "/";
                }
                exeCommandFolderParse += foldername + folderPath;
                i++;
            }

            if (!(getfolder.Substring(getfolder.Length - 1) == "/" || 
                getfolder.Substring(getfolder.Length - 1) == "\\"))
            {
                    exeCommandFolderParse = exeCommandFolderParse + "/";
             
            }
            return exeCommandFolderParse;
        }
       
        public void SettingReadRsyncLogFilePath()
        {
            rsyncLogFileName = RsyncSetting.XmlSetting.exeCommandLogFile;
            rsyncLogPath = RsyncSetting.XmlSetting.exeCommandGetLogPath;
            rsyncLogLocation = GetRsynLogPath(rsyncLogPath) +
                RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearMonthDate) + "_" + 
                rsyncLogFileName;
            rsyncTargetHost = RsyncSetting.XmlSetting.exeCommandToTarget;
            rsyncSourceFolder = RsyncSetting.XmlSetting.exeCommandFromSourceFolder;
            rsyncTargetFolder = RsyncSetting.XmlSetting.exeCommandToTargetFolder;
        }

        public void WriteRsyncLog()
        {
            if (rsyncReadLogFileLength)
            {
                DownloadCenterRsyncLog =
                            "[Download Center][Success]Read Rsync.exe Log File Length : " +
                            rsynLogLength;
            }
            else if (rsyncReadLogFile)
            {
                DownloadCenterRsyncLog =
                            "[Download Center][Success]Rsync Log File has new record,log length : " +
                            rsyncLogFile.Length;
                
            }
            else if (rsyncReadLogFileException)
            {
                DownloadCenterRsyncLog = "[Download Center][Exception]" + rsyncReadLogFileExceptionMessage;
            }
            

            if (DownloadCenterRsyncLog != null)
            {
                RsyncDateTime.WriteLog(DownloadCenterRsyncLog);
            }
        }

        public override void ReadNewRsyncLogRecord(int rsyncLogOriginal)
        {
            string rsyncLogType;

            if (rsyncLogFile.Length - rsynLogLength > 0 && rsynLogLength != -1)
            {
                for (int i = rsynLogLength; i < rsyncLogFile.Length; i++)
                {
                    ReadErrorLog(i, rsyncLogFile);
                    rsyncLogLine = rsyncLogFile[i];
                    rsyncLogType = VerifyRsyncBackupType(rsyncLogLine);
                    GetRsyncDetail(rsyncLogType);
                }
            }
        }

        public void ReadRsyncLog(bool rsyncLogNewRecord)
        {   
            SettingReadRsyncLogFilePath();           
            ReadLogFile(rsyncLogLocation,rsyncLogNewRecord);
            rsynLogLength = rsyncLogFileLength;
            WriteRsyncLog();
        }

        public bool ReadErrorLog(int logLength,string[] newLogMessage)
        {
            int newLogLength;
            string rsyncError;

            if (logLength + 1 == rsyncLogFile.Length)
            {
                newLogLength = logLength;
            }
            else
            {
                newLogLength = logLength + 1;
            }

            rsyncError = newLogMessage[newLogLength];

            rsyncLogErrorStatus = SearchSubString(ref rsyncError, "failed: Permission denied");

            return rsyncLogErrorStatus;
        }

        public void ParseRsyncFileDetail(string getLogLine,string RsyncTypeAction)
        {
            string replaceTargetDefault = "";
            string[] replaceFolder;
            string getFolderName = "";

            if(getLogLine.Length > 9 && getLogLine.Substring(0, 9) == "cygdrive/")
            {
                replaceTargetDefault = getLogLine.Substring(9, 1) + ":/";
                getLogLine = getLogLine.Substring(11, getLogLine.Length - 11);               
            }
            else if(getLogLine.Length > 9 && getLogLine.Substring(1, 9) == "cygdrive/")
            {
                replaceTargetDefault = getLogLine.Substring(10, 1) + ":/";
                getLogLine = getLogLine.Substring(12, getLogLine.Length - 12);
            }
            else if (SearchSubString(ref rsyncSourceFolder, "//") || SearchSubString(ref rsyncSourceFolder, "\\\\"))
            {
                replaceTargetDefault = "//";
            }
            else
            {
                replaceTargetDefault = "//";
            }

            replaceFolder = getLogLine.Split(new char[] { '\\', '/' });

            if (RsyncTypeAction == "del")
            {
                if (rsyncTargetFolder.Substring(rsyncTargetFolder.Length - 1) == "/" || 
                    rsyncTargetFolder.Substring(rsyncTargetFolder.Length - 1) == "\\")
                {
                    getFolderName += rsyncSourceFolder.Replace("\\","/");
                }
                else
                {
                    getFolderName += replaceTargetDefault + "/";
                }
            }
            else
            {
                getFolderName += replaceTargetDefault + replaceFolder[0] + "/";
            }

            for(int j = 1; j < replaceFolder.Length - 1; j++)
            {
                getFolderName += replaceFolder[j] + "/";
            }

            getFolderName += replaceFolder[replaceFolder.Length - 1];
            RsyncFileLog = getFolderName;
        }

        public void ParseRsyncLogDetail(string type)
        {
            string[] rsyncLogTime;
            rsyncLogTime = RsyncLogDetail[0].Split(new char[] { ' ' });
            RsyncCommandStart = rsyncLogTime[0] + " " + 
                rsyncLogTime[1];
            ParseRsyncFileDetail(RsyncLogDetail[1],type);
        }
        
        public  static bool SearchSubString(ref string ParentString, string SubString)
        {
            return 0 <= ParentString.IndexOf(SubString, StringComparison.OrdinalIgnoreCase);
        }

        public string[] ApiFileListID()
        {
            string[] rsyncFileIDList = null;

            if (RsyncSetting.XmlSetting.apiRsyncFileListCheckSum != "")
            {
                if (RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Substring(RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Length - 1) == ",")
                {
                    RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Substring(0, RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Length - 1);
                }
                rsyncFileIDList = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Split(new char[] { ',' });
            }
            return rsyncFileIDList;
        }

        public void GetRsyncDetail(string rsyncParseLogeType)
        {
            string[] errorList;
            string errorFile = "";

            if (null != RsyncLogDetail && RsyncLogDetail.Length >= 2)
            {
                if (SearchSubString(ref rsyncParseLogeType, "send") && !rsyncLogErrorStatus)
                {
                    ParseRsyncLogDetail("send");
                    DownloadCenterRsyncLog = "[Download Center][Success]" +
                        RsyncCommandStart + " " +
                        RsyncFileLog + " already Copy to " +
                        rsyncTargetHost;
                    if (!SearchSubString(ref RsyncSetting.XmlSetting.apiRsyncFileListCheckSum,RsyncSetting.XmlSetting.apiRsyncFileID))
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Replace(RsyncSetting.XmlSetting.apiRsyncFileID + ",","");
                    }
                    RsyncHtmlLog = RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#EEF4FD\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + RsyncCommandStart + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + RsyncSetting.XmlSetting.apiRsyncSourceFolder + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">already Copy to " + "Storage " + RsyncSetting.XmlSetting.apiRsyncTargetFolder + "</font></td></tr>";
                }
                else if (SearchSubString(ref rsyncParseLogeType, "del"))
                {
                    ParseRsyncLogDetail("del");
                    DownloadCenterRsyncLog = "[Download Center][Success]" +
                        RsyncCommandStart + " " +
                        RsyncFileLog + " Delete From " +
                        rsyncTargetHost;
                    RsyncHtmlLog = RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#FFFF99\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + RsyncCommandStart + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + RsyncSetting.XmlSetting.apiRsyncSourceFolder + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">Delete From " + rsyncTargetHost + "</font></td></tr>";
                }
                else if (SearchSubString(ref rsyncParseLogeType, "link_stat"))
                {
                    ParseRsyncLogDetail("link_stat");
                    DownloadCenterRsyncLog = "[Download Center][Error]" +
                        RsyncCommandStart + " " +
                        RsyncFileLog + " Don't Copy From " +
                        rsyncTargetHost;
                    if (SearchSubString(ref RsyncSetting.XmlSetting.apiRsyncFileListCheckSum, RsyncSetting.XmlSetting.apiRsyncFileID))
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Replace(RsyncSetting.XmlSetting.apiRsyncFileID + ",", "");
                    }
                    RsyncHtmlLog = RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#f28c9b\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + RsyncCommandStart + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + RsyncSetting.XmlSetting.apiRsyncSourceFolder + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> No such file or directory  </font></td></tr>";
                }
                else if(SearchSubString(ref rsyncParseLogeType, "failed: File exists"))
                {
                    RsyncHtmlLog = "<font color = \"#FF0000\" size = \"2\" face = \"Verdana, sans-serif\">Target folder is not exists";
                }
                else if(SearchSubString(ref rsyncParseLogeType, "failed: No such host or network path"))
                {
                   ParseRsyncLogDetail("rsync: change_dir ");
                   errorList = RsyncLogDetail[0].Split(new string[] { "rsync: change_dir "}, StringSplitOptions.None);
                   errorMessage = errorList[1].Replace("\"","");
                   if(SearchSubString(ref RsyncSetting.XmlSetting.apiRsyncSourceFolder,errorMessage))
                   {
                        errorFile = RsyncSetting.XmlSetting.apiRsyncSourceFolder;
                   }
                   else
                   {
                        errorFile = errorMessage;
                   }
                    if (errorMessage != "" && SearchSubString(ref RsyncSetting.XmlSetting.apiRsyncFileListCheckSum, RsyncSetting.XmlSetting.apiRsyncFileID))
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Replace(RsyncSetting.XmlSetting.apiRsyncFileID + ",", "");
                    }
                    RsyncHtmlLog = RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#f28c9b\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + RsyncCommandStart + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + RsyncSetting.XmlSetting.apiRsyncSourceFolder + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> No such file or directory  </font></td></tr>";
                }
                else if(SearchSubString(ref rsyncParseLogeType, "rsync: change_dir#3"))
                {
                    ParseRsyncLogDetail("rsync: change_dir#3");
                    DownloadCenterRsyncLog = "[Download Center][Error]" +
                        RsyncCommandStart + " " +
                        RsyncFileLog + " Don't Copy From " +
                        rsyncTargetHost;
                    if (SearchSubString(ref RsyncSetting.XmlSetting.apiRsyncFileListCheckSum, RsyncSetting.XmlSetting.apiRsyncFileID))
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum.Replace(RsyncSetting.XmlSetting.apiRsyncFileID + ",", "");
                    }
                    RsyncHtmlLog = RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#f28c9b\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + RsyncCommandStart + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + RsyncSetting.XmlSetting.apiRsyncSourceFolder + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> No such file or directory  </font></td></tr>";
                }
                else if (rsyncLogErrorStatus)
                {
                    RsyncHtmlLog = "<font color = \"#FF0000\" size = \"2\" face = \"Verdana, sans-serif\">Target folder permission denied";
                }
                

                if(DownloadCenterRsyncLog != null )
                    RsyncDateTime.WriteLog(DownloadCenterRsyncLog);
                DownloadCenterRsyncLog = null;
            }
        }

        public string VerifyRsyncBackupType(string rsyncBackupLogType)
        {
            string verifyRsyncLogType = "";

            foreach (string getRsyncType in RsyncType)
            {
                if (rsyncBackupLogType.IndexOf(getRsyncType, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    verifyRsyncLogType = getRsyncType;

                    if (getRsyncType == "rsync: link_stat ")
                    {
                        rsyncBackupLogType = rsyncBackupLogType.Replace("\"", "");
                        RsyncLogDetail = rsyncBackupLogType.Split(new string[] { getRsyncType, " failed: No such file or directory" }, StringSplitOptions.None);
                    }
                    else
                    {
                        RsyncLogDetail = rsyncBackupLogType.Split(new string[] { getRsyncType }, StringSplitOptions.None);
                    }
                }
            }
            return verifyRsyncLogType;
        }
    }
}
