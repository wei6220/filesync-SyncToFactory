using System;
using System.IO;

namespace DownloadCenterRsyncBaseLog
{
    abstract class RsyncBaseLog
    {
        protected string[] rsyncLogFile { get; set; }
        protected string rsyncReadLogFileExceptionMessage { get; set; }
        protected int rsyncLogFileLength { get; set; }
        protected int rsyncLogFileOrigLength { get; set; }
        protected bool rsyncReadLogFileLength { get; set; }
        protected bool rsyncReadLogFile { get; set; }
        protected bool rsyncReadLogFileException { get; set; }
        protected  object _lockWrite = new object();

        virtual public void ReadNewRsyncLogRecord(int rsyncLogLocaton)
        {
            for (int i = rsyncLogLocaton; i < rsyncLogFile.Length; i++)
            { 
                Console.WriteLine(rsyncLogFile[i]);
            }
        }

        public void ReadLogFileLine(int originalLogLength,bool oldLogRecord)
        {
            if (oldLogRecord)
            {
                rsyncReadLogFileLength = true;
                rsyncReadLogFile = false;            
                rsyncLogFileLength = rsyncLogFile.Length;
                rsyncLogFileOrigLength = rsyncLogFileLength;
            }
            else
            {
                rsyncReadLogFileLength = false;
                rsyncReadLogFile = true;
                ReadNewRsyncLogRecord(rsyncLogFileOrigLength);
            }
        }

        public void ReadLogFile(string logPath,bool readLogLength)
        {
            try
            {
                lock (_lockWrite)
                {
                    if (File.Exists(logPath))
                    {
                        using (FileStream file = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            rsyncLogFile = File.ReadAllLines(logPath);
                            ReadLogFileLine(rsyncLogFileLength, readLogLength);
                            file.Close();
                        }
                    }
                    else
                    {
                        rsyncLogFileLength = 0;
                    }
                }
            }
            catch (Exception e)
            {
                rsyncReadLogFileLength = false;
                rsyncReadLogFile = false;
                rsyncReadLogFileException = true;
                rsyncLogFileLength = -1;
                rsyncReadLogFileExceptionMessage = e.Message;
                Console.WriteLine(e.Message);
            }
        }
    }
}
