using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UninstallAction
    {
        public string Alias { get; set; }

        public string PackageName { get; set; }

        public string RunAt { get; set; }//NOTE Should this default to "install"

        public bool Undo { get; set; }//NOTE: Should thid default to "False"?

        public XmlNode XmlData { get; set; }
    }
}