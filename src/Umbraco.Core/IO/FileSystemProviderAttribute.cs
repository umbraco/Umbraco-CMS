using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FileSystemProviderAttribute : Attribute
    {
        public string Alias { get; private set; }

        public FileSystemProviderAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
