using CommonLibrary;
using DownloadCenterRsyncSetting;
using System;

namespace DownloadCenterFileListApi
{
    class FileListApi
    {
        private static string FileListApiURL;
        private static string responseFileList;
        private static string reponseFileApi;

        public static string GetFileList()
        {
            try
            {
                FileListApiURL = RsyncSetting.GetRsyncConf("RsyncSetting/FileListApi", "URL");

                HttpHelper http = new HttpHelper();

                responseFileList = http.Get(FileListApiURL);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
               
            return responseFileList;
        }
    }
}
