using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates a model name for a specified content alias.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class RenameContentTypeAttribute : Attribute
    {
        public RenameContentTypeAttribute(string alias, string name)
        {}
    }
}
