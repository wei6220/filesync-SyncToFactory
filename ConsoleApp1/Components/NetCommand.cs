using System;

namespace DownloadCenter
{
    class NetCommand : BaseCommand
    {
        private string _cmdResponse = "";
        public bool ExeLoginCmd()
        {
            _cmdResponse = "";
            string netCmd = "", netPath = "";
            netCmd = "net " + @"use \\" + RsyncSetting.Config.SourceServerIP + " " + RsyncSetting.Config.SourceServerPassword + " /user:" + RsyncSetting.Config.SourceServerLogin;
            netPath = @"C:\Windows\System32";
            Log.WriteLog("Cmd execute: " + netCmd);
            ExeCmd(netCmd, netPath);

            return GetLoginTargetServerStatus(RsyncSetting.Config.SourceServerIP, RsyncSetting.Config.SourceServerLogin);
        }

        public bool ExeLoginFactoryCmd()
        {
            _cmdResponse = "";
            string netCmd = "", netPath = "";
            netPath = @"C:\Windows\System32";

            netCmd = "net " + @"use \\" + RsyncSetting.Config.TargetServerIp + " /delete";
            Log.WriteLog("Cmd execute: " + netCmd);
            ExeCmd(netCmd, netPath);

            netCmd = "net " + @"use \\" + RsyncSetting.Config.TargetServerIp + " " + RsyncSetting.Config.TargetServerPassword + " /user:" + RsyncSetting.Config.TargetServerLogin;
            Log.WriteLog("Cmd execute: " + netCmd);
            ExeCmd(netCmd, netPath);

            return GetLoginTargetServerStatus(RsyncSetting.Config.TargetServerIp, RsyncSetting.Config.TargetServerLogin);
        }

        public override void ResponseCmdMessage(string cmdResponse)
        {
            if (!string.IsNullOrEmpty(cmdResponse))
            {
                _cmdResponse = cmdResponse;
                Log.WriteLog("Cmd response:" + cmdResponse);
            }
        }

        public override void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                Log.WriteLog(exceptionMessage, Log.Type.Exception);
            }
        }

        private bool GetLoginTargetServerStatus(string host, string user)
        {
            bool isLogin;
            if (_cmdResponse.Contains("成功") || _cmdResponse.Contains("successfully"))
            {
                isLogin = true;
                Log.WriteLog(user + " success login " + host);
            }
            else
            {
                isLogin = false;
                Log.WriteLog(user + " fail login server " + host, Log.Type.Failed);
            }
            return isLogin;
        }

       
    }
}
