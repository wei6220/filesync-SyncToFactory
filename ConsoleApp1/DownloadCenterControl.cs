using System;
using DownloadCenterRsyncDateTime;
using DownloadCenterNetCommand;
using DownloadCenterRsyncToStorSimpleLog;
using DownloadCenterRsyncSchedule;
using DownloadCenterConsoleIFolder;
using DownloadCenterConsoleFolder;
using DownloadCenterRsyncSetting;
using DownloadCenterRsyncCommand;
using DownloadCenterFileListApi;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace DownloadCenter
{
    class DownloadCenterControl
    {
        public static int rsyncLogLength;
        public static string apiFileSource, apiFileTarget, apiRsyncCreateTargetFolder;
        public static string apiRsyncSourceFolder, apiRsyncTargetFolder, emailRsyncTargetFolder;
        public static dynamic rsyncLog, rsyncExe, rsyncSchedule;
        public static string[] apiFolderList,rsyncFileUpdateIDs;
        public static bool apiTargetFolderStatus;

        static void Main(string[] args)
        {
            int scheduleLogLength;
            string scheduleFinishStatus = "";

            rsyncLog = new DownloadCenterLog();
            rsyncExe = new RsyncCommand();
            rsyncSchedule = new RsyncSchedule();

            RsyncSetting.XmlSetting.exeCommandStartTime = RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange);
            scheduleLogLength = rsyncSchedule.ReadScheduleLog();
       
            RsyncDateTime.WriteLog("[Download Center][Success]Rsync Schedule Start");
            RsyncSetting.SettingRsyncExeConfig();

            if (scheduleLogLength >= 0 || scheduleLogLength == -2)
            {
                try
                {
                    if (scheduleLogLength == -2)
                    {
                        RsyncDateTime.WriteLog("[Download Center][Success]Kill Rsync Progress");
                        DeleteProcess(rsyncLog);
                    }
                }
                catch
                {
                   DeleteProcess(rsyncLog);
                }

                LoginTargetServer();

                scheduleFinishStatus = "[Download Center][Success]Rsync Schedule Finish";
            }
            else
            {
                if (scheduleLogLength == -3)
                {
                    rsyncLogLength = -3;
                    scheduleFinishStatus = "[Download Center][Waring]Rsync Schedule wait for anthor schedule is finsih";
                }
            }

            rsyncExe.SendEmailLog(rsyncLog, rsyncLogLength);
            RsyncDateTime.WriteLog(scheduleFinishStatus);
        }

        public static void DeleteProcess(dynamic multiThreadLog)
        {
            int processID = 0;
            bool threadReadRsyncLogStatus;

            Process currentProcess = Process.GetCurrentProcess();


            foreach (Process processRsyncSchedule in Process.GetProcessesByName("DownloadCenterRsync"))
            {
                if (currentProcess.Id != processRsyncSchedule.Id)
                {
                    processRsyncSchedule.Kill();
                }
            }

            foreach (Process processRsync in Process.GetProcessesByName("rsync"))
            {
                processID++;

                if (processID >= 2)
                {
                    processRsync.Kill();
                }

            }


            while (threadReadRsyncLogStatus = multiThreadLog.ReadThreadsRsyncLog())
            {
                Thread.Sleep(1000);
            }

        
        }

        private static void VerifyEmptyFolderList(ref int emptyFolderNum, int rysncFolderNum )
        {
            if (apiTargetFolderStatus == false && rysncFolderNum == 0)
            {
                return;
            }
            else
            {
                if (apiFolderList[rysncFolderNum] == "")
                {
                    return;
                }
                else
                {
                    emptyFolderNum++;

                    if (emptyFolderNum == 1)
                    {
                        return;
                    }
                    else
                    {
                        apiRsyncSourceFolder = apiRsyncSourceFolder + apiFolderList[rysncFolderNum] + "/";
                    }
                }
            }
        }

        private static void GetRsyncFolder()
        {
            int emptyLine = 0;

            for (int folderNum = 0; folderNum < apiFolderList.Length; folderNum++)
            {
                if (folderNum == apiFolderList.Length - 1)
                {
                    apiRsyncSourceFolder = apiRsyncSourceFolder + apiFolderList[folderNum];
                    apiRsyncTargetFolder = apiRsyncTargetFolder + apiFolderList[folderNum];
                    emailRsyncTargetFolder = emailRsyncTargetFolder + apiFolderList[folderNum];
                }
                else
                {
                    if (apiTargetFolderStatus)
                    {
                        apiRsyncCreateTargetFolder = apiRsyncCreateTargetFolder + apiFolderList[folderNum] + "/";
                        if (apiFolderList[folderNum] == "")
                        {
                            continue;
                        }
                        else
                        {
                            emailRsyncTargetFolder = emailRsyncTargetFolder + apiFolderList[folderNum] + "/";
                            apiRsyncTargetFolder = apiRsyncTargetFolder + apiFolderList[folderNum] + "/";
                        }
                    }

                    VerifyEmptyFolderList(ref emptyLine, folderNum);
                } 
            }
        }

        private static void VerifyApiFileListFolder()
        {
            if (apiTargetFolderStatus)
            {
                apiRsyncCreateTargetFolder = RsyncSetting.XmlSetting.networkMountDevice + "/";

                emailRsyncTargetFolder = RsyncSetting.XmlSetting.networkMountDevice;

                apiRsyncTargetFolder = "/cygdrive/" + RsyncSetting.XmlSetting.networkMountDevice;
                apiRsyncTargetFolder = apiRsyncTargetFolder.Replace(":", "");

                if (apiRsyncTargetFolder.Substring(apiRsyncTargetFolder.Length - 1) == "/" ||
                apiRsyncTargetFolder.Substring(apiRsyncTargetFolder.Length - 1) == "\\")
                {
                    apiRsyncCreateTargetFolder = apiRsyncCreateTargetFolder.Substring(0, apiRsyncCreateTargetFolder.Length - 1);

                }
                else
                {
                    apiRsyncCreateTargetFolder = apiRsyncCreateTargetFolder + "/";
                    apiRsyncTargetFolder = apiRsyncTargetFolder + "/";
                }
            }
            else
            {
                apiRsyncSourceFolder = "//" + RsyncSetting.XmlSetting.targetServerIP + "/";
            }
        }

        private static string FileList(ref string rsyncFolderList, bool targetFolderStatus)
        {
            string rsyncFolder;

            apiTargetFolderStatus = targetFolderStatus;

            apiFolderList = rsyncFolderList.Split(new string[] { "\\", "/" }, StringSplitOptions.None);

            VerifyApiFileListFolder();

            GetRsyncFolder();
            

            if (apiTargetFolderStatus)
            {
                RsyncSetting.XmlSetting.apiRsyncTargetFolder = emailRsyncTargetFolder;
                rsyncFolder = apiRsyncTargetFolder;
            }
            else
            {
                RsyncSetting.XmlSetting.apiRsyncSourceFolder = apiRsyncSourceFolder;
                rsyncFolder = apiRsyncSourceFolder;
            }
            return rsyncFolder;
        }

        private static void WriteFolderLog(string logMessage)
        {
            if (logMessage != "")
            {
                RsyncDateTime.WriteLog(logMessage);
            }
        }

        private static bool GetFileListApi()
        {
            var getFileList = FileListApi.GetFileList();
            JObject fileList;

            if (getFileList != null)
            {
                fileList = (JObject)JsonConvert.DeserializeObject(getFileList);
            }
            else
            {
                fileList = null;
            }
            int fileIndex = 0;
            string rsyncSourceFolder = "", rsyncTargetFolder = "", targetCreateFolderMessage, rsyncLogMessage = "";
            bool apiFileListStatus;

            if (fileList != null)
            {
                apiFileListStatus = true;

                RsyncSetting.SettingNetworkDevice();
                IFolder rsyncFolder = new Folder();
                rsyncLogMessage = rsyncFolder.CreateFolder(RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/DownloadCenterLog", "Path"), false);

                WriteFolderLog(rsyncLogMessage);

                var syncFactoryData = fileList["syncdata"];
                var syncFactoryDeleteData = fileList["deletedata"];

                foreach (var syncFileList in syncFactoryData)
                {

                    apiFileSource = syncFileList["source"].ToString();
                    apiFileTarget = syncFileList["target"].ToString();

                    RsyncSetting.XmlSetting.apiRsyncFileID = syncFileList["id"].ToString();

                    RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum + syncFileList["id"] + ",";

                    rsyncSourceFolder = FileList(ref apiFileSource, false);
                    rsyncTargetFolder = FileList(ref apiFileTarget, true);
                    try
                    {
                        FileInfo file = new FileInfo(rsyncSourceFolder);
                        RsyncSetting.XmlSetting.apiRsyncFileSize = file.Length.ToString();
                        targetCreateFolderMessage = rsyncFolder.CreateFolder(apiRsyncCreateTargetFolder, false);
                        WriteFolderLog(targetCreateFolderMessage);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains(apiFileSource))
                        {
                            RsyncSetting.XmlSetting.apiRsyncFileSize = "-1";
                        }
                        else
                        {
                            RsyncSetting.XmlSetting.apiRsyncFileSize = "-2";
                        }
                    
                        Console.WriteLine(e.Message);
                    }

                    rsyncExe.ExeCommand(rsyncSourceFolder, rsyncTargetFolder,false);
                    fileIndex++;
                }

                foreach (var syncFileDeleteList in syncFactoryDeleteData)
                {

                    apiFileTarget = syncFileDeleteList["target"].ToString();

                    RsyncSetting.XmlSetting.apiRsyncFileID = syncFileDeleteList["id"].ToString();

                    rsyncSourceFolder = FileList(ref apiFileSource, false);
                    rsyncTargetFolder = FileList(ref apiFileTarget, true);

                    rsyncExe.ExeCommand(emailRsyncTargetFolder, apiRsyncCreateTargetFolder, true);
                    fileIndex++;
                }
            }
            else
            {
                apiFileListStatus = false;
            }
            return apiFileListStatus;
        }

        public static void LoginTargetServer()
        {
            bool login;

            dynamic exe = new NetCommand();
            login = exe.ExeCommand();

            if (login)
            {
                rsyncLogLength = 0;
                ResponseFileListApi();
            }
            else
            {
                rsyncLogLength = -1;
            }
        }

        public static void ResponseFileListApi()
        {
            if (GetFileListApi())
            {
                rsyncLogLength = rsyncLog.GetRsyncLogLength;
            }
            else
            {
                rsyncLogLength = -4;
            }
        }
    }
}
