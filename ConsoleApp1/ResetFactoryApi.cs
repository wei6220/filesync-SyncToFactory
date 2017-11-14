using CommonLibrary;
using DownloadCenterRsyncSetting;
using System;

namespace DownloadCenterRsyncResetFactoryGatway
{
    class ResetFactoryApi
    {
        private static string RestFactoryGatwayApiURL;
        private static string responseRunbook;

        public static string RestFactoryGatway()
        {
            try
            {
                RestFactoryGatwayApiURL = RsyncSetting.GetRsyncConf("RsyncSetting/RestFactoryGatway", "URL");

                HttpHelper http = new HttpHelper();

                responseRunbook = http.Post(RestFactoryGatwayApiURL, "", HttpHelper.ContnetTypeEnum.Json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return responseRunbook;
        }
    }
}
