using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DownloadCenter
{
    abstract class BaseCommand
    {
        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        protected void ExeCmd(string commandOption, string commandPath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
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
                var cmdError = process.StandardError.ReadToEnd();
                if (cmdError != "")
                {
                    Console.WriteLine(cmdError);
                }

                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                ErrorExceptionHandle(e.Message);
            }
        }

        virtual public void ErrorExceptionHandle(string exceptionMessage)
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                Log.WriteLog(exceptionMessage, Log.Type.Exception);
            }
        }

        virtual public bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                try
                {
                    Log.WriteLog("Console window closing, death imminent");
                }
                catch (Exception e)
                {
                    Log.WriteLog(e.Message, Log.Type.Exception);
                }
            }
            return true;
        }

        private void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                ResponseCmdMessage(outLine.Data);
            }
        }

        virtual public void ResponseCmdMessage(string cmdResponse)
        {
            Console.WriteLine(cmdResponse);
        }
       
    }
}
