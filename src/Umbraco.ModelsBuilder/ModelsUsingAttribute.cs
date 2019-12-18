using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Umbraco.ModelsBuilder;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates namespaces that should be used in generated models (in using clauses).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class ModelsUsingAttribute : Attribute
    {
        public ModelsUsingAttribute(string usingNamespace)
        {}
    }
}

