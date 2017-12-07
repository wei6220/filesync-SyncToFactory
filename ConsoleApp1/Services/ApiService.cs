using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenter
{
    class ApiService
    {
        public string GetFileList()
        {
            string result = "";
            try
            {
                var http = new HttpHelper();
                result = http.Get(RsyncSetting.Config.FileListApiUrl);
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message,Log.Type.Exception);
            }
            return result;
        }

        public string UpdateFileStatus(List<UpdateInfo> info)
        {
            string message = ""; ;
            try
            {
                var http = new HttpHelper();
                var result = http.Post(RsyncSetting.Config.FileUpdateStatusApiUrl, info, HttpHelper.ContnetTypeEnum.Json);
                if (string.IsNullOrEmpty(result))
                {
                    message = "Post UpdateStatus Api is failed.";
                    Log.WriteLog(message, Log.Type.Failed);
                }
                else
                {
                    var resultJson = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(result);
                    if (resultJson["status"] != null && resultJson["status"].ToLower() == "error")
                    {
                        message = resultJson["message"];
                        Log.WriteLog(message, Log.Type.Failed);
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                Log.WriteLog(message, Log.Type.Exception);
            }
            return message;
        }
        public struct UpdateInfo
        {
            public string id { get; set; }
            public string size { get; set; }
            public string status { get; set; }
            public string message { get; set; }
        }

        public string RestFactoryGatway()
        {
            string result = "";
            try
            {
                HttpHelper http = new HttpHelper();
                result = http.Post(RsyncSetting.Config.RestFactoryGatwayUrl, "", HttpHelper.ContnetTypeEnum.Json);
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, Log.Type.Exception);
            }
            return result;
        }

    }
}
