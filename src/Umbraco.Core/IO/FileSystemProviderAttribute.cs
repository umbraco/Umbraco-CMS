using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal class FileSystemProviderAttribute : Attribute
    {
        public string Alias { get; set; }

        public FileSystemProviderAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
