using CommonLibrary;
using DownloadCenterRsyncSetting;
using DownloadCenterRsyncToStorSimpleLog;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenterFileUpdateFinishApi
{
    class FileUpdateFinishApi
    {
        public static string fileupUpdateFinishURL;
        public static string fileListID;
        public static Dictionary<string, string> responseFileIDApi;
        public static string responseMessage;

        public static string FileUpdateFinish(List<DownloadCenterLog> fileListID)
        {
            try
            { 
                fileupUpdateFinishURL = RsyncSetting.GetRsyncConf("RsyncSetting/FileUpdateFinishApi", "URL");

                HttpHelper http = new HttpHelper();


                var responseFileIDList = http.Post(fileupUpdateFinishURL, fileListID, HttpHelper.ContnetTypeEnum.Json);

                if (responseFileIDList == null)
                {
                    responseMessage = "[Download Center][Error]Post Update File Api is Error";
                }
                else
                {
                    responseFileIDApi = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(responseFileIDList);
                    responseMessage = "[Download Center][Success]" + responseFileIDApi["message"];
                }
                return responseMessage;
            }
            catch(Exception e)
            {

            }
            return responseMessage;
        }
    }
}
