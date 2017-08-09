using CommonLibrary;
using DownloadCenterRsyncSetting;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DownloadCenterFileListApi
{
    class FileListApi
    {
        private static string FileListApiURL;
        private static string responseFileList;
        private static List<dynamic> reponseFileApi;
        public static List<dynamic> GetFileList()
        {
            FileListApiURL = RsyncSetting.GetRsyncConf("RsyncSetting/FileListApi", "URL");
            
            HttpHelper http = new HttpHelper();
 
            responseFileList = http.Get(FileListApiURL);
            if (responseFileList == null)
            {
                reponseFileApi = null;
            }
            else
            {
                reponseFileApi = new JavaScriptSerializer().Deserialize<List<dynamic>>(responseFileList);
                
            }
            return reponseFileApi;
        }
    }
}
