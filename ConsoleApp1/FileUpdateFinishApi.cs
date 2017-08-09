using CommonLibrary;
using DownloadCenterRsyncDateTime;
using DownloadCenterRsyncSetting;
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

        public static string FileUpdateFinish(string[] apiFileListID)
        {
            fileupUpdateFinishURL = RsyncSetting.GetRsyncConf("RsyncSetting/FileUpdateFinishApi", "URL");

            HttpHelper http = new HttpHelper();
           
            if (apiFileListID != null)
            {
                fileListID = "[";
                for (int i = 0; i < apiFileListID.Length; i++)
                {
                    if (i != apiFileListID.Length - 1)
                    {
                        fileListID = fileListID + "\"" + apiFileListID[i] + "\",";
                    }
                    else
                    {
                        fileListID = fileListID + "\"" + apiFileListID[i] + "\"]";
                    }
                }
            }
            else
            {
                fileListID = "Not get any file id";
            }
            
            RsyncDateTime.WriteLog("[Download Center][Success]Api File ID " + fileListID );
           // fileListID = null;
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
    }
}
