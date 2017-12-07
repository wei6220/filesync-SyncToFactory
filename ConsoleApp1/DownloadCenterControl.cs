using System;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;

namespace DownloadCenter
{
    class DownloadCenterControl
    {
        static void Main(string[] args)
        {
            RsyncSetting.SetConfigSettings();
            RsyncSetting.RuntimeSettings.ScheduleStartTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange);
            Log.WriteLog("#######################################################################");
            Log.WriteLog("Rsync schedule start.");
            SyncResultRecords.Init();
            if (CheckScheduleReady("DownloadCenterRsync"))
            {
                try
                {
                    //1.取api list 
                    var api = new ApiService();
                    var result = api.GetFileList();
                    if (string.IsNullOrEmpty(result))
                        throw new Exception("Get file list api failed.");
                    var fileList = JsonConvert.DeserializeObject<dynamic>(result);
                    var syncDataList = (IEnumerable<dynamic>)fileList.syncdata;
                    var delDataList = (IEnumerable<dynamic>)fileList.deletedata;
                    Log.WriteLog("Will to deal records sync("+ syncDataList.Count() + ") + del("+ delDataList.Count() + ")");
                    if (syncDataList.Count() + delDataList.Count() > 0)
                    {
                        //2.確認工廠連線
                        if (RsyncSetting.Config.TargetServerTest
                            || !CheckConnectionOK(RsyncSetting.Config.TargetServerIp))
                        {
                            Log.WriteLog("Reset factory vnet");
                            api.RestFactoryGatway();
                            if (!CheckConnectionOK(RsyncSetting.Config.TargetServerIp))
                                throw new Exception("Connect to factory (IP:" + RsyncSetting.Config.TargetServerIp + ") failed.");
                        }

                        //3.確認StoreSimple連線
                        var cmd = new NetCommand();
                        if (!cmd.ExeLoginCmd())
                            throw new Exception("Not login source server :" + RsyncSetting.Config.SourceServerIP);
                        if (!cmd.ExeLoginFactoryCmd())
                            throw new Exception("Not login target server :" + RsyncSetting.Config.TargetServerIp);

                        //4.rsync
                        SyncData(syncDataList);
                        DeleteData(delDataList);

                        if (SyncResultRecords.All().Count > 0)
                            RsyncSetting.RuntimeSettings.RsyncResultMessage = GenerateResultMessage(SyncResultRecords.All());
                    }
                    else
                    {
                        Log.WriteLog("No file need to be sync");
                        RsyncSetting.RuntimeSettings.RsyncResultMessage =
                            "<font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">"
                            + "No file need to be sync </font>";
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLog(e.Message, Log.Type.Exception);
                    RsyncSetting.RuntimeSettings.RsyncResultMessage =
                            "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">" + e.Message + "</font>";
                }
            }
            else
            {
                Log.WriteLog("Wait for anoher schedule is finish", Log.Type.Failed);
                RsyncSetting.RuntimeSettings.RsyncResultMessage =
                    "<font color = \"#c61919\" size = \"2\" face = \"Verdana, sans-serif\">"
                    + "Waiting for anoher schedule is finish</font>";
            }

            Mail mail = new Mail();
            mail.Send();
            Log.WriteLog("Rsync schedule finish.");
        }

        private static bool CheckScheduleReady(string processName)
        {
            bool isReady = true;
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process processRsyncSchedule in Process.GetProcessesByName(processName))
                {
                    if (currentProcess.Id != processRsyncSchedule.Id)
                        isReady = false;
                }

                if (!isReady)
                    Log.WriteLog("The another DownloadCenter.exe is execute, wait for anoher schedule finish");
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return isReady;
        }

        private static bool CheckConnectionOK(string host)
        {
            bool isConnect = false;
            var timeout = Convert.ToInt32(RsyncSetting.Config.PingTimeout);
            var repeat = Convert.ToInt32(RsyncSetting.Config.PingRepeat);
            Ping ping = new Ping();
            PingReply reply;
            int count = 0;
            while (!isConnect && count++ < repeat)
            {
                Log.WriteLog("Ping(" + count + ") " + host + " ...");
                reply = ping.Send(host, timeout);
                if (reply.Status == IPStatus.Success)
                {
                    Log.WriteLog("Ping " + host + " is OK.");
                    isConnect = true;
                }
            }
            return isConnect;
        }

        private static string GetReplaceHostPath(string originPath)
        {
            string newPath = "";
            var chunks = originPath.Split('\\');
            chunks.ToList().ForEach(e =>
            {
                if (!string.IsNullOrWhiteSpace(newPath))
                    newPath += "\\";
                newPath += e;
            });

            if (newPath.Split('\\')[0].ToLower() == RsyncSetting.Config.SourceServerHost)
                newPath = RsyncSetting.Config.SourceServerIP + newPath.Substring(newPath.IndexOf('\\'));
            else if (newPath.Split('\\')[0].ToLower() == RsyncSetting.Config.TargetServerHost)
                newPath = RsyncSetting.Config.TargetServerIp + newPath.Substring(newPath.IndexOf('\\'));
            return newPath = "\\\\" + newPath;
        }

        private static void SyncData(IEnumerable<dynamic> syncDataList)
        {
            var errorMessage = "";
            string id, size, sourcePath, targetPath;
            var rsyncCmd = new RsyncCommand();
            SyncResultRecords.SyncResult resultRecord;
            foreach (var syncData in syncDataList)
            {
                id = size = sourcePath = targetPath = "";
                try
                {
                    id = syncData.id;
                    size = "";
                    sourcePath = GetReplaceHostPath(syncData.source.ToString());
                    targetPath = GetReplaceHostPath(syncData.target.ToString());
                    Log.WriteLog("Start sync to factory.(id:" + id + ")");
                    Log.WriteLog(sourcePath + " -> " + targetPath);
                    if (!File.Exists(sourcePath))
                    {
                        Log.WriteLog("No such file or directory.", Log.Type.Failed);
                        resultRecord = new SyncResultRecords.SyncResult
                        {
                            Id = id,
                            Size = size,
                            FinishTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange),
                            SourcePath = sourcePath,
                            TargetPath = targetPath,
                            Status = "Failed",
                            Message = "No such file or directory.",
                        };
                        errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                    }
                    else
                    {
                        FileInfo file = new FileInfo(sourcePath);
                        size = file.Length.ToString();

                        rsyncCmd.ExeSyncCmd(sourcePath, targetPath);
                        if (rsyncCmd.ErrorMessage != "")
                            throw new Exception(rsyncCmd.ErrorMessage);
                        resultRecord = new SyncResultRecords.SyncResult
                        {
                            Id = id,
                            Size = size,
                            FinishTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange),
                            SourcePath = sourcePath,
                            TargetPath = targetPath,
                            Status = "Success",
                            Message = targetPath.Replace("\\", "/") + " is already sync to factory.",
                        };
                        errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLog(e.Message, Log.Type.Exception);
                    resultRecord = new SyncResultRecords.SyncResult
                    {
                        Id = id,
                        Size = size,
                        FinishTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange),
                        SourcePath = sourcePath,
                        TargetPath = targetPath,
                        Status = "Failed",
                        Message = targetPath.Replace("\\", "/") + " sync is failed.",
                    };
                    errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                }

                if (errorMessage != "")
                {
                    resultRecord.Status = "Failed";
                    resultRecord.Message += (resultRecord.Message != "" ? " " : "") + errorMessage;
                }

                SyncResultRecords.Add(resultRecord);
                Log.WriteLog("Sync to factory is finish.");
            }
        }

        private static void DeleteData(IEnumerable<dynamic> delDataList)
        {
            var errorMessage = "";
            string id, size, sourcePath, targetPath;
            var rsyncCmd = new RsyncCommand();
            SyncResultRecords.SyncResult resultRecord;
            foreach (var delData in delDataList)
            {
                id = size = sourcePath = targetPath = "";
                try
                {
                    id = delData.id;
                    size = "";
                    targetPath = GetReplaceHostPath(delData.target.ToString());
                    Log.WriteLog("Start delete from factory.(id:" + id + ")");
                    Log.WriteLog("targetPath :" + targetPath);

                    var targetDir = string.Join("\\", targetPath.Split('\\').Take(targetPath.Split('\\').Length - 1));
                    if (Directory.Exists(targetDir))
                        Directory.Delete(targetDir, true);

                    resultRecord = new SyncResultRecords.SyncResult
                    {
                        Id = id,
                        Size = size,
                        FinishTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange),
                        SourcePath = sourcePath,
                        TargetPath = targetPath,
                        Status = "Success",
                        Message = targetPath.Replace("\\", "/") + " deleted is success.",
                    };
                    errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                }
                catch (Exception e)
                {
                    Log.WriteLog(e.Message, Log.Type.Exception);
                    resultRecord = new SyncResultRecords.SyncResult
                    {
                        Id = id,
                        Size = size,
                        FinishTime = RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange),
                        SourcePath = sourcePath,
                        TargetPath = targetPath,
                        Status = "Failed",
                        Message = targetPath.Replace("\\", "/") + " deleted is failed.",
                    };
                    errorMessage = UpdateStatus(new List<SyncResultRecords.SyncResult> { resultRecord });
                }

                if (errorMessage != "")
                {
                    resultRecord.Status = "Failed";
                    resultRecord.Message += (resultRecord.Message != "" ? " " : "") + errorMessage;
                }
                SyncResultRecords.Add(resultRecord);
                Log.WriteLog("Delete from factory is finish.");
            }
        }

        private static string UpdateStatus(List<SyncResultRecords.SyncResult> syncResultList)
        {
            var errorMsg = "";
            var updateList = new List<ApiService.UpdateInfo>();
            syncResultList.ForEach(e =>
            {
                updateList.Add(new ApiService.UpdateInfo
                {
                    id = e.Id,
                    size = e.Size,
                    status = (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception") ? "error" : "success",
                    message = e.Message,
                });
            });

            if (updateList.Count > 0)
            {
                ApiService api = new ApiService();
                errorMsg = api.UpdateFileStatus(updateList);
            }
            return errorMsg;
        }

        private static string GenerateResultMessage(List<SyncResultRecords.SyncResult> results)
        {
            string message = "";
            var count = 0;
            results.ForEach(e =>
            {
                count++;
                if (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception")
                {
                    message += "<tr><td bgcolor = \"#EEF4FD\" width = \"2%\"><font size = \"2\" face = \"Verdana, sans-serif\">" + count + "</font></td>"
                          + "<td bgcolor = \"#f28c9b\" width = \"12%\"><font size = \"2\" face = \"Verdana, sans-serif\">" + e.FinishTime + "</font></td>"
                          + "<td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + e.SourcePath.Replace("\\", "/") + "</font></td>"
                          + "<td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\"> " + e.Message + "</font></td></tr>";
                }
                else
                {
                    message += "<tr><td bgcolor = \"#EEF4FD\" width = \"2%\"><font size = \"2\" face = \"Verdana, sans-serif\">" + count + "</font></td>"
                          + "<td bgcolor = \"#EEF4FD\" width = \"12%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + e.FinishTime + "</font></td>"
                          + "<td width = \"55%\"><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + e.SourcePath.Replace("\\", "/") + "</font></td>"
                          + "<td width = \"30%\" ><font color = \"#4A72A2\" size = \"2\" face = \"Verdana, sans-serif\">" + e.Message + "</font></td></tr>";
                }
            });
            return message;
        }
    }
}
