using System;
using DownloadCenterRsyncToStorSimpleLog;
using DownloadCenterRsyncDateTime;
using DownloadCenterRsyncSetting;
using DownloadCenterRsyncEmailTemplate;
using DownloadCenterSendEmail;
using DownloadCenterRsyncBaseCmd;
using System.IO;

namespace DownloadCenterRsyncCommand
{
    class RsyncCommand : RsyncBaseCmd
    {
        private int copyFile = 0;
        private string htmlLogMessage, htmlLogTemplate, rsyncHtmlLog, sendMailLog;

        public void SettingRsyncHtmlLogTemplate(ref string targetFoldr)
        {
            rsyncHtmlLog = htmlLogTemplate.Replace("{RsyncFromHost}", RsyncSetting.XmlSetting.exeCommandFromSource);
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncFromHostFolder}", RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/RsyncSource", "Folder").Replace("\\", "/"));
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncToDestination}", RsyncSetting.XmlSetting.exeCommandToTarget);
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncToDestinationFolder}", RsyncSetting.GetRsyncConf("RsyncSetting/NetworkDevice", "Path"));
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncLog}", htmlLogMessage);
            rsyncHtmlLog = rsyncHtmlLog.Replace("{count}", copyFile.ToString());
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncStartTime}", RsyncSetting.XmlSetting.exeCommandStartTime);
            rsyncHtmlLog = rsyncHtmlLog.Replace("{RsyncFinishTime}", RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange));
        }

        public string RsyncMailLogTargetFolder()
        {
            string rsyncLogTargetFolder;

            rsyncLogTargetFolder = RsyncSetting.GetRsyncConf("RsyncSetting/RsyncExe/RsyncTarget", "Folder");

            if (rsyncLogTargetFolder.IndexOf("/", StringComparison.OrdinalIgnoreCase) >= 0 || rsyncLogTargetFolder.IndexOf("\\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                rsyncLogTargetFolder = rsyncLogTargetFolder.Replace("\\", "/");
            }

            SettingRsyncHtmlLogTemplate(ref rsyncLogTargetFolder);

            return rsyncLogTargetFolder;
        }

        public void RsyncMailHtmlLog(ref int rsyncLogLength)
        {
            if (rsyncLogLength == -1)
            {
                htmlLogMessage = "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">The NetWork has problem,fail login Target";
            }
            else if (rsyncLogLength == -3)
            {
                htmlLogMessage = "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Rsyc Schedule wait for finish";
            }
            else if (rsyncLogLength == -4)
            {
                htmlLogMessage = "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">Get Api File List is null";
            }
            else if (rsyncLogLength == -5)
            {
                htmlLogMessage = "<font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">No File Sync To Factory";
            }
            else
            {
                if (DownloadCenterLog.RsyncHtmlLog == null)
                {
                    htmlLogMessage = "<font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">No Change File and Folder";
                }
                else
                {
                    htmlLogMessage = DownloadCenterLog.RsyncHtmlLog;
                }
            }
        }

        public void SendEmailLog(dynamic exeRsyncLog, int exeRysncLogLength)
        {
            string rsyncMailLogDestination;

            dynamic htmlTemplate = new DownloadCenterEmailTemplate();

            RsyncMailHtmlLog(ref exeRysncLogLength);

            htmlLogTemplate = htmlTemplate.HtmlTemplate().Replace('"', '\"');

            rsyncMailLogDestination = RsyncMailLogTargetFolder();
            
            
            sendMailLog = DownloadCenterEmail.SendEmailLog(rsyncHtmlLog);

            RsyncDateTime.WriteLog(sendMailLog);
        }

        public override void ErrorExceptionHandle(string rsyncException)
        {
            if (!String.IsNullOrEmpty(rsyncException))
            {
                RsyncDateTime.WriteLog("[Download Center][Exception]" + rsyncException);
                Console.WriteLine(rsyncException);
            }
        }


        public void ExeCommand(string apiSoucre, string apiTarget,bool deleteStatus)
        {
            string exeCommandTime = "",rsyncExe = "",rsyncExePath = "", deleteFactoryTime ;

            if (deleteStatus)
            {
                dynamic rsyncLog = new DownloadCenterLog();
                deleteFactoryTime = RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearMonthDateTime);
                try
                {
                    Directory.Delete(apiTarget, true);
                    DownloadCenterLog.RsyncHtmlLog = DownloadCenterLog.RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#FFFF99\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + deleteFactoryTime + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + apiSoucre + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">Delete From " + RsyncSetting.XmlSetting.exeCommandToTarget + "</font></td></tr>"; 
                    rsyncLog.UpdateFileList("success","");
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(apiTarget.Replace("/","\\")))
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileSize = "-1";
                        DownloadCenterLog.RsyncHtmlLog = DownloadCenterLog.RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#f28c9b\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + deleteFactoryTime + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + apiSoucre + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> No such file or directory" + "</font></td></tr>";
                        rsyncLog.UpdateFileList("error", e.Message);
                    }
                    else
                    {
                        RsyncSetting.XmlSetting.apiRsyncFileSize = "-2";
                        DownloadCenterLog.RsyncHtmlLog = DownloadCenterLog.RsyncHtmlLog + "<tr><td width = \"22%\" bgcolor = \"#f28c9b\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\" >" + deleteFactoryTime + " </font></td><td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + apiSoucre + "</font></td><td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> networking error" + "</font></td></tr>";
                        rsyncLog.UpdateFileList("error", e.Message);
                    }

                    Console.WriteLine(e.Message);
                }
                
            }
            else
            {
                RsyncSetting.SettingRsyncExeConfig();
                exeCommandTime = RsyncDateTime.GetTimeNow(RsyncDateTime.TimeFormatType.YearMonthDate);
                rsyncExe = RsyncSetting.XmlSetting.exeCommand + " " + RsyncSetting.XmlSetting.exeCommandOption + " "

                    + apiSoucre + " "
                    + apiTarget + " "

                    + " --log-file=" + RsyncSetting.XmlSetting.exeCommandLogPath + exeCommandTime + "_" + RsyncSetting.XmlSetting.exeCommandLogFile
                    + " --log-file-format=\"%i %o %f\"";
                rsyncExePath = RsyncSetting.XmlSetting.exeCommandPath;

                dynamic rsyncLog = new DownloadCenterLog();
                rsyncLog.ReadRsyncLog(true);

                RsyncDateTime.WriteLog("[Download Center][Success]Rsync Command Start");
                BaseCommand(rsyncExe, rsyncExePath);

                if (cmdRsyncFinish)
                {
                    rsyncLog.ReadRsyncLog(false);
                    RsyncDateTime.WriteLog("[Download Center][Success]Rsync Command Finish");
                }
            }
        }

        public void WriteConsoleLine(ref string cmdLine)
        {
            if (RegexName(cmdLine, "%"))
            {
                if (cmdLine.Contains("100%") && cmdLine.Contains("(xfer#"))
                {
                    Console.WriteLine(cmdLine);
                }
                else
                {
                    Console.Write(cmdLine + "\r");
                }
            }
            else
            {
                if (RegexName(cmdLine, "deleting"))
                {
                    copyFile++;
                }
                else if (!(RegexName(cmdLine, "sending incremental") ||
                          RegexName(cmdLine, "total size is") ||
                          RegexName(cmdLine, "sent [0-9]") ||
                          cmdLine == "./") ||
                          cmdLine.Contains("(xfer#"))
                {
                    copyFile++;
                }
                Console.WriteLine(cmdLine);
            }
        }

        public override void ResponseCmdMessage(string cmdResponse)
        {
            if (!String.IsNullOrEmpty(cmdResponse))
            {
                WriteConsoleLine(ref cmdResponse);
            }
        }
    }
}
