using DownloadCenterRsyncBaseCmd;
using DownloadCenterRsyncDateTime;
using DownloadCenterRsyncSetting;
using System;

namespace DownloadCenterNetCommand
{
    class NetCommand : RsyncBaseCmd
    {
        private string successOutput = ""; 
        private string logStatus = "",logMessage = "";
        private string cmdNetTargetIP = "", cmdNetTargetPassword = "", cmdNetLogin = "";
        private bool netCmdLoginStatus;

        private void LoginTaegetServer()
        {
            if (cmdError.Contains("錯誤") || cmdError.Contains("error"))
            {
                logStatus = "[Error]";
                logMessage = "Fail Login";
                netCmdLoginStatus = false;
                
            }
            else if (successOutput.Contains("成功") || successOutput.Contains("successfully"))
            {
                logStatus = "[Success]";
                logMessage = "Success Login Target";
                netCmdLoginStatus = true;
            }
            else
            {
                logStatus = "[Unknown]";
                logMessage = "Fail Login Target";
                netCmdLoginStatus = false;
            }

            Console.WriteLine(cmdNetLogin + " " + logMessage + " " + cmdNetTargetIP);
            RsyncDateTime.WriteLog("[Download Center]" + logStatus + cmdNetLogin + " " + logMessage + " " + cmdNetTargetIP);
        }

        public bool ExeCommand()
        {
            string netCmd = "",netPath = "";

            RsyncSetting.SettingTargetServer();
            cmdNetTargetIP = RsyncSetting.XmlSetting.targetServerIP;
            cmdNetTargetPassword = RsyncSetting.XmlSetting.targetServerPassword;
            cmdNetLogin = RsyncSetting.XmlSetting.targetServerLogin;

            netCmd = "net " + @"use \\" + cmdNetTargetIP + " " + cmdNetTargetPassword + " /user:misd\\" + cmdNetLogin;
            netPath = @"C:\Windows\System32";
            BaseCommand(netCmd, netPath);

            LoginTaegetServer();

            return netCmdLoginStatus;
        }

        public override void ResponseCmdMessage(string cmdResponse)
        {
            if (!String.IsNullOrEmpty(cmdResponse))
            {
                successOutput = cmdResponse;
            }
        }

        public override void ErrorExceptionHandle(string exceptionMessage)
        {
            if(exceptionMessage != "")
            {
                RsyncDateTime.WriteLog("[Download Center][Exception]" + exceptionMessage);
            }
        }
    }
}
