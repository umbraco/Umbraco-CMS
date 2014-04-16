using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class InstallAction
    {
        public string Alias { get; set; }

        public string PackageName { get; set; }

        public string RunAt { get; set; }//NOTE Should this default to "install"

        public bool Undo { get; set; }

        public XmlNode XmlData { get; set; }
    }
}