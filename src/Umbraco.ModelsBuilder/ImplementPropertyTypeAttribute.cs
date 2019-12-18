using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates that a property implements a given property alias.
    /// </summary>
    /// <remarks>And therefore it should not be generated.</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ImplementPropertyTypeAttribute : Attribute
    {
        public ImplementPropertyTypeAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; private set; }
    }
}
