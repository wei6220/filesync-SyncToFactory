using DownloadCenterRsyncBaseSetting;
using DownloadCenterRsyncDateTime;
using System;
using System.Xml;

namespace DownloadCenterRsyncSetting
{
    class RsyncSetting : RsyncBaseSetting
    {
        static XmlDocument doc;
        
        public static string GetRsyncConf(string rsyncSetting, string value)
        {
            string data = "";

            try
            {
                doc = new XmlDocument();
                doc.Load("RsyncSetting.xml");
                XmlNode main = doc.SelectSingleNode(rsyncSetting);
                XmlElement element = (XmlElement)main;
                data = element.GetAttribute(value);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return data;
        }

        public static void SettingScheduleLog()
        {
            XmlSetting.rsyncScheduleLogPath = RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/DownloadCenterLog", "Path");
            XmlSetting.rsyncScheduleLogFile = RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/DownloadCenterLog", "File");
        }

        public static void SettingTargetServer()
        {
            XmlSetting.targetServerIP  = RsyncSetting.GetRsyncConf("RsyncSetting/TargetServer", "Ip");
            XmlSetting.targetServerLogin = RsyncSetting.GetRsyncConf("RsyncSetting/TargetServer", "Login");
            XmlSetting.targetServerPassword = RsyncSetting.GetRsyncConf("RsyncSetting/TargetServer", "Pwd");
        }

        public static void SettingNetworkDevice()
        {
            XmlSetting.networkMountDevice = GetRsyncConf("RsyncSetting/NetworkDevice", "Path");
            XmlSetting.targetServerIP = GetRsyncConf("RsyncSetting/TargetServer", "Ip");
        }

        public static void SettingRsyncExeConfig()
        {
            XmlSetting.exeCommand = GetRsyncConf("RsyncSetting/RsyncExe", "Command");
            XmlSetting.exeCommandPath = GetRsyncConf("RsyncSetting/RsyncExe", "Path");
            XmlSetting.exeCommandOption = GetRsyncConf("RsyncSetting/RsyncExe/Option/RsyncParameter", "Option");
            XmlSetting.exeCommandFromSource = GetRsyncConf("RsyncSetting/RsyncExe/RsyncSource", "Host");
            XmlSetting.exeCommandFromSourceFolder = GetRsyncConf("RsyncSetting/RsyncExe/RsyncSource", "Folder");
            XmlSetting.exeCommandToTarget = GetRsyncConf("RsyncSetting/RsyncExe/RsyncTarget", "Host");
            XmlSetting.exeCommandToTargetFolder = GetRsyncConf("RsyncSetting/RsyncExe/RsyncTarget", "Folder");
            XmlSetting.exeCommandLogFile = GetRsyncConf("RsyncSetting/RsyncExe/RsyncLog", "File");
            XmlSetting.exeCommandLogPath = GetRsyncConf("RsyncSetting/RsyncExe/RsyncLog", "Path");
            XmlSetting.exeCommandGetLogPath = GetRsyncConf("RsyncSetting/RsyncExe/RsyncLog", "Path");
            SettingRsyncFolder(ref XmlSetting.exeCommandFromSourceFolder, false);
            SettingRsyncFolder(ref XmlSetting.exeCommandToTargetFolder, false);
            SettingRsyncFolder(ref XmlSetting.exeCommandLogPath, true);
        }

        public struct XmlSetting
        {
            public static string exeCommand = "";
            public static string exeCommandPath = "";
            public static string exeCommandOption = "";
            public static string exeCommandFromSource = "";
            public static string exeCommandToTarget = "";
            public static string exeCommandFromSourceFolder = "";
            public static string exeCommandToTargetFolder = "";
            public static string exeCommandLogPath = "";
            public static string exeCommandLogFile = "";
            public static string exeCommandGetLogPath = "";
            public static string exeCommandStartTime = "";
            public static string exeCommandFinishTime = "";
            public static string networkMountDevice = "";
            public static string targetServerIP = "";
            public static string apiRsyncSourceFolder = "";
            public static string apiRsyncTargetFolder = "";
            public static string apiRsyncFileID = "";
            public static string apiRsyncFileListCheckSum = "";
            public static string targetServerIP1 = "";
            public static string targetServerLogin = "";
            public static string targetServerPassword = "";
            public static string rsyncScheduleLogPath = "";
            public static string rsyncScheduleLogFile = "";
        };

        public static void RsyncCygdriveFolder(ref string cygdriveBackupFolder, ref string rysncFolderPath)
        {
            string[] rsyncSyncFolderList;
            int i = 0;

            rsyncSyncFolderList = cygdriveBackupFolder.Split(new char[] { '\\', '/' });

            foreach (string folderName in rsyncSyncFolderList)
            {
                if (i++ != 0)
                {
                    rysncFolderPath += "/";
                }
                rysncFolderPath += folderName.Replace(":", "");
            }
        }

        public static void SettingRsyncFolder(ref string rsyncFolder, Boolean rsyncLogFolderFlag)
        {
            string rsyncCommandSyncFolder;

            if (rsyncFolder.Length >= 2)
            {
                if (rsyncFolder.IndexOf("\\\\", 0, 2) >= 0 || rsyncFolder.IndexOf("//", 0, 2) >= 0)
                {
                    rsyncCommandSyncFolder = "";
                }
                else
                {
                    rsyncCommandSyncFolder = "/cygdrive/";
                }
            }
            else
            {
                rsyncCommandSyncFolder = "";
            }
       
            RsyncCygdriveFolder(ref rsyncFolder, ref rsyncCommandSyncFolder);
            
            if (!(rsyncFolder.Substring(rsyncFolder.Length - 1) == "/" || rsyncFolder.Substring(rsyncFolder.Length - 1) == "\\"))
            {
                if (rsyncLogFolderFlag)
                {
                    rsyncCommandSyncFolder = rsyncCommandSyncFolder + "/";
                }
            }
            rsyncFolder = rsyncCommandSyncFolder;
        }

        public string GetRsyncConfigSetting(string rsyncSetting, string rsyncValue)
        {
            string xmlData = "";

            xmlData = GetRsyncConfig(rsyncSetting, rsyncValue);

            if (xmlConfigError)
            {
                RsyncDateTime.WriteLog("[Download Center][Exception]" + xmlData);
            }

            return xmlData;
        }
    }
}
