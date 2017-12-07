using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DownloadCenter
{
    class RsyncCommand : BaseCommand
    {
        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } }
        public void ExeSyncCmd(string sourcePath, string targetPath)
        {
            _errorMessage = "File sync is not finish.";
          
            if (!Directory.Exists(RsyncSetting.Config.RsyncExeLogPath))
                Directory.CreateDirectory(RsyncSetting.Config.RsyncExeLogPath);

            var targetDir = string.Join("\\", targetPath.Split('\\').Take(targetPath.Split('\\').Length - 1));
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            Directory.CreateDirectory(targetDir);

            string rsyncExe = "", rsyncExePath = "";
            rsyncExe = RsyncSetting.Config.RsyncExeName + " " + RsyncSetting.Config.RsyncExeOption + " \""
                + RsyncSetting.ConvertToRsyncFormat(sourcePath) + "\" \""
                + RsyncSetting.ConvertToRsyncFormat(targetPath) + "\" "
                + " --log-file=" + RsyncSetting.GetRsyncLogFileFullPath()
                + " --log-file-format=\"%i %o %f\"";
            rsyncExePath = RsyncSetting.Config.RsyncExePath;
            Log.WriteLog("Cmd execute: " + rsyncExe);
            ExeCmd(rsyncExe, rsyncExePath);
        }

        override public void ResponseCmdMessage(string response)
        {
            if (!string.IsNullOrEmpty(response))
            {
                if (RegexName(response, "100%"))
                {
                    _errorMessage = "";
                    Log.WriteLog("Cmd response: " + response);
                }
                else if (!RegexName(response, "%"))
                {
                    Log.WriteLog("Cmd response: " + response);
                }
            }
        }

        override public void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                Log.WriteLog(exceptionMessage, Log.Type.Exception);
                Process.GetProcesses().Where(pr => pr.ProcessName == "rsync").ToList().ForEach(e => {
                    e.Kill();
                });
                throw new Exception(exceptionMessage);
            }
        }

        override public bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                try
                {
                    Log.WriteLog("Console window closing, death imminent");
                    Process.GetProcesses().Where(pr => pr.ProcessName == "rsync").ToList().ForEach(e=> {
                        e.Kill();
                    });
                    Log.WriteLog("Rsync process clear");
                }
                catch (Exception e)
                {
                    Log.WriteLog(e.Message, Log.Type.Exception);
                }
            }
            return true;
        }

        private bool RegexName(string str, string reg)
        {
            return Regex.IsMatch(str, reg);
        }
    }
}
