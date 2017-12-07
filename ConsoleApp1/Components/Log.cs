using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadCenter
{
    class Log
    {
        public enum Type
        {
            Info,
            Failed,
            Exception,
        };

        public static void WriteLog(string log, Type type = Type.Info)
        {
            //string prefix = "[Schedule:" + RsyncSetting.RuntimeSettings.ScheduleID + "]";
            string prefix = "";
            switch (type)
            {
                case Type.Info:
                    prefix += "[Info]";
                    break;
                case Type.Failed:
                    prefix += "[Failed]";
                    break;
                case Type.Exception:
                    prefix += "[Exception]";
                    break;
            }
            Console.WriteLine(prefix + " " + log);
            var path = RsyncSetting.Config.LogPath + RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearMonth) + "\\";
            CommonLibrary.LogHelper.Write(prefix + " " + log,
                RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearMonthDate) + ".txt",
                path);
        }
    }
}
