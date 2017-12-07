using System;
using System.Xml;

namespace DownloadCenter
{
    class RsyncSetting
    {
        #region Get Xml Setting
        public struct Config
        {
            public static string LogPath = "";
            public static string LogFileName = "";
            public static string RsyncExeName = "";
            public static string RsyncExePath = "";
            public static string RsyncExeOption = "";
            public static string SourceServerHost = "";
            public static string SourceServerFolder = "";
            public static string SourceServerIP = "";
            public static string SourceServerLogin = "";
            public static string SourceServerPassword = "";
            public static string TargetServerHost = "";
            public static string TargetServerFolder = "";
            public static string TargetServerIp = "";
            public static string TargetServerLogin = "";
            public static string TargetServerPassword = "";
            public static bool TargetServerTest = false;

            public static string RsyncExeLogPath = "";
            public static string RsyncExeLogFileName = "";
            public static string FileListApiUrl = "";
            public static string FileUpdateStatusApiUrl = "";
            public static string PingTimeout = "";
            public static string PingRepeat = "";
            public static string RestFactoryGatwayUrl = "";
            public static string RestFactoryGatwayDueDate = "";
            public static string RestFactoryGatwayNotification = "";
            public static string EmailUrl = "";
            public static string EmailCC = "";
            public static string EmailSubject = "";
        };
        private static XmlDocument _settingDoc;
        private static void LoadSettingDoc()
        {
            _settingDoc = new XmlDocument();
            _settingDoc.Load("RsyncSetting.xml");
        }
        private static string GetSettingDocAttrValue(string nodePath, string attrKey)
        {
            string data = "";
            try
            {
                if (_settingDoc == null)
                    LoadSettingDoc();
                XmlElement element = (XmlElement)_settingDoc.SelectSingleNode(nodePath);
                data = element.GetAttribute(attrKey);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return data;
        }
        public static void SetConfigSettings() {
            SetTargetServerConfig();
            SetRsyncExeConfig();
            SetApiUrlConfig();
            SetRestFactoryGatwayConfig();
            SetEmailConfig();
            SetOtherConfig();
        }
        public static void SetTargetServerConfig()
        {
            Config.SourceServerIP = GetSettingDocAttrValue("RsyncSetting/SourceServer", "Ip");
            Config.SourceServerLogin = GetSettingDocAttrValue("RsyncSetting/SourceServer", "Login");
            Config.SourceServerPassword = GetSettingDocAttrValue("RsyncSetting/SourceServer", "Pwd");
            Config.SourceServerHost = GetSettingDocAttrValue("RsyncSetting/SourceServer", "Host");
            Config.SourceServerFolder = GetSettingDocAttrValue("RsyncSetting/SourceServer", "Folder");

            Config.TargetServerIp = GetSettingDocAttrValue("RsyncSetting/TargetServer", "IP");
            Config.TargetServerLogin = GetSettingDocAttrValue("RsyncSetting/TargetServer", "Login");
            Config.TargetServerPassword = GetSettingDocAttrValue("RsyncSetting/TargetServer", "Pwd");
            Config.TargetServerHost = GetSettingDocAttrValue("RsyncSetting/TargetServer", "Host");
            Config.TargetServerFolder = GetSettingDocAttrValue("RsyncSetting/TargetServer", "Folder");
            Config.TargetServerTest = (GetSettingDocAttrValue("RsyncSetting/TargetServer", "Test").ToLower() == "true") ? true : false;
        }
        public static void SetRsyncExeConfig()
        {
            Config.RsyncExeName = GetSettingDocAttrValue("RsyncSetting/RsyncExe", "Command");
            Config.RsyncExePath = GetSettingDocAttrValue("RsyncSetting/RsyncExe", "Path");
            Config.RsyncExeOption = GetSettingDocAttrValue("RsyncSetting/RsyncExe/Option/RsyncParameter", "Option");
            Config.RsyncExeLogFileName = GetSettingDocAttrValue("RsyncSetting/RsyncExe/RsyncLog", "File");
            Config.RsyncExeLogPath = GetSettingDocAttrValue("RsyncSetting/RsyncExe/RsyncLog", "Path");
        }
        public static void SetApiUrlConfig()
        {
            Config.FileListApiUrl = GetSettingDocAttrValue("RsyncSetting/FileListApi", "URL");
            Config.FileUpdateStatusApiUrl = GetSettingDocAttrValue("RsyncSetting/FileUpdateFinishApi", "URL");
        }
        public static void SetRestFactoryGatwayConfig()
        {
            Config.RestFactoryGatwayUrl = GetSettingDocAttrValue("RsyncSetting/RestFactoryGatway", "URL");
            Config.RestFactoryGatwayDueDate = GetSettingDocAttrValue("RsyncSetting/RestFactoryGatway", "DueDate");
            Config.RestFactoryGatwayNotification = GetSettingDocAttrValue("RsyncSetting/RestFactoryGatway", "Notification");
        }
        public static void SetEmailConfig()
        {
            Config.EmailUrl = GetSettingDocAttrValue("RsyncSetting/Email", "URL");
            Config.EmailCC = GetSettingDocAttrValue("RsyncSetting/Email/Option", "CC");
            Config.EmailSubject = GetSettingDocAttrValue("RsyncSetting/Email/Subject", "Content");
        }
        public static void SetOtherConfig()
        {
            Config.PingTimeout = GetSettingDocAttrValue("RsyncSetting/Ping", "Timeout");
            Config.PingRepeat = GetSettingDocAttrValue("RsyncSetting/Ping", "Repeat");
            Config.LogPath = GetSettingDocAttrValue("RsyncSetting/DownloadCenterLog", "Path");
            Config.LogFileName = GetSettingDocAttrValue("RsyncSetting/DownloadCenterLog", "File");
        }
        #endregion

        public struct RuntimeSettings
        {
            public static string ScheduleStartTime = "";
            public static string RsyncResultMessage = "";
        }

        public static string GetRsyncLogFileFullPath()
        {
            string LogPath = Config.RsyncExeLogPath;
            if (!(LogPath.Substring(LogPath.Length - 1) == "/"
                || LogPath.Substring(LogPath.Length - 1) == "\\"))
            {
                LogPath += "\\";
            }
            return ConvertToRsyncFormat(LogPath +
                RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearMonthDate) + "_" + Config.RsyncExeLogFileName);
        }

        public static string ConvertToRsyncFormat(string originPath)
        {
            return originPath.Replace('\\', '/');
        }
    }
}
