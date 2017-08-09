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
using DownloadCenterFileUpdateFinishApi;
using DownloadCenterRsyncProcess;

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
                        RsyncProcess.DeleteProcess(rsyncLog);
                    }
                }
                catch
                {
                    RsyncProcess.DeleteProcess(rsyncLog);
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

            UpdateFileList();
            rsyncExe.SendEmailLog(rsyncLog, rsyncLogLength);
            RsyncDateTime.WriteLog(scheduleFinishStatus);
        }

        private static void UpdateFileList()
        {
            string rsyncUpdateLog;

            rsyncFileUpdateIDs = rsyncLog.ApiFileListID();

            if (rsyncFileUpdateIDs == null)
            {
                rsyncUpdateLog = FileUpdateFinishApi.FileUpdateFinish(rsyncFileUpdateIDs);
            }
            else
            {
                rsyncUpdateLog = FileUpdateFinishApi.FileUpdateFinish(rsyncFileUpdateIDs);
            }

            RsyncDateTime.WriteLog(rsyncUpdateLog);
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
                apiRsyncCreateTargetFolder = RsyncSetting.XmlSetting.networkMountDevice;

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
            var fileList = FileListApi.GetFileList();
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

                foreach (var list in fileList)
                {
                    apiFileSource = fileList[fileIndex]["source"];
                    apiFileTarget = fileList[fileIndex]["target"];

                    RsyncSetting.XmlSetting.apiRsyncFileID = fileList[fileIndex]["ID"];
                    RsyncSetting.XmlSetting.apiRsyncFileListCheckSum = RsyncSetting.XmlSetting.apiRsyncFileListCheckSum + fileList[fileIndex]["ID"] + ",";

                    rsyncSourceFolder = FileList(ref apiFileSource, false);
                    rsyncTargetFolder = FileList(ref apiFileTarget, true);

                    targetCreateFolderMessage = rsyncFolder.CreateFolder(apiRsyncCreateTargetFolder,false);
                    WriteFolderLog(targetCreateFolderMessage);

                    rsyncExe.ExeCommand(rsyncSourceFolder, rsyncTargetFolder);
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
