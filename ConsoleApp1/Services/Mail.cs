using CommonLibrary;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System;
using System.IO;

namespace DownloadCenter
{
    class Mail
    {
        public void Send()
        {
            string ccList = "";
            try
            {
                string[] ccGroup = RsyncSetting.Config.EmailCC.Split(new char[] { ',' });
                for (int i = 0; i < ccGroup.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(ccGroup[i]))
                        continue;
                    if (!string.IsNullOrWhiteSpace(ccList))
                        ccList += ",";
                    ccList += "\"" + ccGroup[i] + "\"";
                }

                bool isImportant = false;
                SyncResultRecords.All().ForEach(e =>
                {
                    if (e.Status.ToLower() == "failed" || e.Status.ToLower() == "exception")
                        isImportant = true;
                });

                string postContent = "{ 'subject' : '" + RsyncSetting.Config.EmailSubject + "'"
                     + ", 'content' : '" + GetContent() + "'"
                     + ", 'To':[" + ccList + "]"
                     + ", 'Cc':'null'"
                     + ", 'Bcc':'null'"
                     + (isImportant ? ", 'Priority':'high'" : "")
                     + " }";

                HttpHelper http = new HttpHelper();
                var result = http.Post(RsyncSetting.Config.EmailUrl, postContent, HttpHelper.ContnetTypeEnum.Json);
                if (result == null)
                {
                    Log.WriteLog("Send mail fail.", Log.Type.Failed);
                }
                else
                {
                    var EmailMessageLog = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(result);
                    Log.WriteLog("Send mail result :" + EmailMessageLog["message"]);
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
        }

        private string GetContent()
        {
            var content = GetHtmlTemplate().Replace('"', '\"');
            content = content.Replace("{RsyncFromHost}", RsyncSetting.Config.SourceServerHost);
            content = content.Replace("{RsyncFromHostFolder}", "");
            content = content.Replace("{RsyncToDestination}", RsyncSetting.Config.TargetServerHost);
            content = content.Replace("{RsyncToDestinationFolder}", "");
            content = content.Replace("{RsyncLog}", RsyncSetting.RuntimeSettings.RsyncResultMessage);
            content = content.Replace("{count}", "");
            content = content.Replace("{RsyncStartTime}", RsyncSetting.RuntimeSettings.ScheduleStartTime);
            content = content.Replace("{RsyncFinishTime}", RsyncDateTime.GetNow(RsyncDateTime.TimeFormatType.YearSMonthSDateTimeChange));
            return content;
        }

        private string GetHtmlTemplate()
        {
            string fileContent = "";
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.GetFileName("RsyncLogTemplate.html")))
                {
                    fileContent = file.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return fileContent;
        }
        
    }
}
