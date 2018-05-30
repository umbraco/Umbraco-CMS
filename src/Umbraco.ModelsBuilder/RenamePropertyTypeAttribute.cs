using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates a model name for a specified property alias.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RenamePropertyTypeAttribute : Attribute
    {
        public RenamePropertyTypeAttribute(string alias, string name)
        {}
    }
}
