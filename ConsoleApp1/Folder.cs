using System;
using System.IO;
using DownloadCenterConsoleIFolder;

namespace DownloadCenterConsoleFolder
{
    class Folder : IFolder
    {
        string folderMessage = "",folderPath = "", folderExceptionMessage;
        bool folderSuccess, folderError, folderException;

        string IFolder.CreateFolder(string folderLocation,bool logFolderType)
        {
            folderPath = folderLocation;

            if (!Directory.Exists(folderLocation))
            {
                try
                {
                    Directory.CreateDirectory(folderLocation);
                    folderSuccess = true;
                }
                catch (Exception e)
                {
                    folderException = true;
                    folderExceptionMessage = e.Message;
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                folderError = true;
            }

            GetFolderLog(logFolderType);
            return folderMessage;
         }

        public void GetFolderLog(bool logFolder)
        {
            if(folderSuccess)
            {
                if(logFolder)
                {
                    folderMessage = "[Download Center][Success]Rsync Log Directory " + folderPath + " was created.";
                }
                else
                {
                    folderMessage = "[Download Center][Success]Target Server Directory " + folderPath + " was created.";
                }
            }
            else if(folderException)
            {
                folderMessage = "[Download Center][Exception]" +  folderExceptionMessage;
            }
            else
            {
                folderMessage = "";
            }
        } 
    }
}
