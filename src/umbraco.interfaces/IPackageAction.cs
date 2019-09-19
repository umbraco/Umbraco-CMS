using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace umbraco.interfaces {
    public interface IPackageAction : IDiscoverable
    {
        bool Execute(string packageName, XmlNode xmlData);
        string Alias();
        bool Undo(string packageName, XmlNode xmlData);
        XmlNode SampleXml();
    }
}
