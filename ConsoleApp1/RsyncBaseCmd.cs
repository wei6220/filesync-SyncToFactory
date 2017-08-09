using DownloadCenterRsyncDateTime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DownloadCenterRsyncBaseCmd
{
    abstract class RsyncBaseCmd
    {
        public string cmdError { get; set; }
        public string cmdSuccess { get; set; }
        public string cmdException { get; set; }
        public bool cmdRsyncFinish { get; set; }

        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
     
        protected void BaseCommand(string commandOption,string commandPath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.Arguments = "/c rsync.exe -r -t -v --progress /cygdrive/c/RsyncBackup/AMD-BaldEagle/CommonDriver/aaa.iso //RD-SW-364/test/aaa666.iso --delete --log-file=/cygdrive/C/Log/20170727rsync.log --log-file-format=\"%i %o %f\"";
                //process.StartInfo.WorkingDirectory = @"C:\grsync\bin";
                process.StartInfo.Arguments = "/c " + commandOption;
                process.StartInfo.WorkingDirectory = commandPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ErrorDialog = true;
                process.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                SetConsoleCtrlHandler(handler, true);
                process.Start();
                process.BeginOutputReadLine();
                cmdError = process.StandardError.ReadToEnd();

                if(cmdError != "")
                {
                    Console.WriteLine(cmdError);
                }

                process.WaitForExit();
                process.Close();
                cmdRsyncFinish = true;
            }
            catch (Exception e)
            {
                cmdException = e.Message;
                ErrorExceptionHandle(cmdException);
                Console.WriteLine(e.Message);
            }
        }

        virtual public void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!String.IsNullOrEmpty(exceptionMessage))
            {
                Console.WriteLine(exceptionMessage);
            }
        }

        public  bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                try
                {
                    Console.WriteLine("Console window closing, death imminent");
                    RsyncDateTime.WriteLog("[Download Center][Waring]Rsync Program was Cancel");
                    var chromeDriverProcesses = Process.GetProcesses().
                                 Where(pr => pr.ProcessName == "rsync");

                    foreach (var process in chromeDriverProcesses)
                    {
                        process.Kill();
                    }
                }
                catch (Exception e)
                {
                    RsyncDateTime.WriteLog("[Download Center][Exception]" + e.Message);
                    Console.WriteLine(e.Message);
                }
            }
            return true;
        }

        virtual public void ResponseCmdMessage(string cmdResponse)
        {
            Console.WriteLine(cmdResponse);
        }

        private void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                ResponseCmdMessage(outLine.Data);
            }
        }

        public static bool RegexName(string str, string reg)
        {
            return Regex.IsMatch(str, reg);
        }
    }
}
