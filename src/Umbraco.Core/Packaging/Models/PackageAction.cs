using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Umbraco.Core.Packaging.Models
{
    internal enum ActionRunAt
    {
        Undefined = 0,
        Install,
        Uninstall
    }

    [Serializable]
    [DataContract(IsReference = true)]
    internal class PackageAction
    {
        private ActionRunAt _runAt;
        private bool? _undo;
        public string Alias { get; set; }

        public string PackageName { get; set; }

        public ActionRunAt RunAt
        {
            get { return _runAt == ActionRunAt.Undefined ? ActionRunAt.Install : _runAt; }
            set { _runAt = value; }
        }

        public bool Undo //NOTE: Should thid default to "False"? but the documentation says default "True" (http://our.umbraco.org/wiki/reference/packaging/package-actions)
        {
            get { return _undo ?? true; }
            set { _undo = value; }
        }
        
        public XElement XmlData { get; set; }
    }
}