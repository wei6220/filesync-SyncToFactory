using CommonLibrary;
using DownloadCenterRsyncSetting;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenterSendEmail
{
    class DownloadCenterEmail
    {
        private static string EmailURL;
        private static string EmailCCList;
        private static string EmailSubjectContent;

        public static string SendEmailLog(string html)
        {
            EmailURL = RsyncSetting.GetRsyncConf("RsyncSetting/Email", "URL");
            EmailCCList = RsyncSetting.GetRsyncConf("RsyncSetting/Email/Option", "CC");
            EmailSubjectContent = RsyncSetting.GetRsyncConf("RsyncSetting/Email/Subject", "Content");

            string[] EmailGroup = EmailCCList.Split(new char[] { ',' });
            string sendEmailCC,emailCC,HtmlLogMessage,responseSendEmail;
            Dictionary<string, string> EmailMessageLog;
            string downloadcenterRsyncEmailLog;

            emailCC = "\""+ EmailGroup[0]+"\"";

            for (int i = 1; i < EmailGroup.Length; i++)
            {
                sendEmailCC = "\""+ EmailGroup[i] +"\"";
                emailCC = emailCC+","+ sendEmailCC;
            }
            HtmlLogMessage = "{ 'subject' : '" + EmailSubjectContent + "','content' : '" + html + "', 'To':[" + emailCC + "],'Cc':'null', 'Bcc':'null'}";
            HttpHelper http = new HttpHelper();
            responseSendEmail = http.Post(EmailURL, HtmlLogMessage, HttpHelper.ContnetTypeEnum.Json);

            if (responseSendEmail == null)
            {
                downloadcenterRsyncEmailLog = "[Download Center][Error]Email Send fail";
            }
            else
            {
                EmailMessageLog = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(responseSendEmail);
                downloadcenterRsyncEmailLog = "[Download Center][Success]Email " + EmailMessageLog["message"];
            }
            return downloadcenterRsyncEmailLog;
        }
    }
}
