using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Umbraco.Cms.Core.Models.Packaging
{
    /// <summary>
    /// The model of the umbraco package data manifest (xml file)
    /// </summary>
    public class CompiledPackage
    {
        public FileInfo PackageFile { get; set; }
        public string Name { get; set; }
        public InstallWarnings Warnings { get; set; } = new InstallWarnings();
        public IEnumerable<XElement> Macros { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> MacroPartialViews { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> Templates { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> Stylesheets { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> Scripts { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> PartialViews { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> DataTypes { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> Languages { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> DictionaryItems { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> DocumentTypes { get; set; } // TODO: make strongly typed
        public IEnumerable<XElement> MediaTypes { get; set; } // TODO: make strongly typed
        public IEnumerable<CompiledPackageContentBase> Documents { get; set; }
        public IEnumerable<CompiledPackageContentBase> Media { get; set; }
    }
}
