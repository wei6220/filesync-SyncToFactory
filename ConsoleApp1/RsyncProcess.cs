using System.Diagnostics;
using System.Threading;

namespace DownloadCenterRsyncProcess
{
    class RsyncProcess
    {
        public static void DeleteProcess(dynamic multiThreadLog)
        {
            int processID = 0;
            bool threadReadRsyncLogStatus;

            Process currentProcess = Process.GetCurrentProcess();

            while (threadReadRsyncLogStatus = multiThreadLog.ReadThreadsRsyncLog())
            {
                Thread.Sleep(1000);
            }

            foreach (Process processRsync in Process.GetProcessesByName("rsync"))
            {
                processID++;

                if (processID >= 2)
                {
                    processRsync.Kill();
                }

            }

            foreach (Process processRsyncSchedule in Process.GetProcessesByName("DownloadCenterRsync"))
            {
                if (currentProcess.Id != processRsyncSchedule.Id)
                {
                    processRsyncSchedule.Kill();
                }
            }
        }
    }
}
