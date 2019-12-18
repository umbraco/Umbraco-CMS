using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Umbraco.Core.Models.Packaging
{
    /// <summary>
    /// Defines a package action declared within a package manifest
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class PackageAction
    {
        private ActionRunAt _runAt;
        private bool? _undo;
        public string Alias { get; set; }

        public string PackageName { get; set; }

        public ActionRunAt RunAt
        {
            get => _runAt == ActionRunAt.Undefined ? ActionRunAt.Install : _runAt;
            set => _runAt = value;
        }

        public bool Undo
        {
            get => _undo ?? true;
            set => _undo = value;
        }

        public XElement XmlData { get; set; }
    }
}
