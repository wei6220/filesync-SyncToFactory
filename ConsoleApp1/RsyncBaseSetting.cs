using System;
using System.Xml;

namespace DownloadCenterRsyncBaseSetting
{
    abstract class RsyncBaseSetting
    {
        string data = "";
        bool xmlConfigStatus;

        public bool xmlConfigError
        {
            get
            {
                return xmlConfigStatus;
            }

            set
            {
                xmlConfigStatus = value;
            }
        }

        virtual public string GetRsyncConfig(string xmlPath, string xmlValue)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("RsyncSetting1.xml");
                XmlNode main = doc.SelectSingleNode(xmlPath);
                XmlElement element = (XmlElement)main;
                data = element.GetAttribute(xmlValue);
            }
            catch (Exception e)
            {
                xmlConfigError = true;
                data = e.Message;
            }

            return data;
        }
    }
}
