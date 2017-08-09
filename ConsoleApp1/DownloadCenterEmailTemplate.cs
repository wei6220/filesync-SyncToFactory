using System;

namespace DownloadCenterRsyncEmailTemplate
{
    class DownloadCenterEmailTemplate
    {
        private string fileContent;

        public string HtmlTemplate()
        {
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.GetFileName("RsyncLogTemplate.html")))
                {
                    fileContent = file.ReadToEnd();
                }
                
            }
            catch (Exception e)
            {
                fileContent = e.Message;
                Console.WriteLine(fileContent);
            }
            return fileContent;
        }
    }
}
