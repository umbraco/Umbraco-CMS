using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building
{
    public class TextBuilderConstants
    {
        public static readonly IDictionary<string, string> TypesMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "System.Int16", "short" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.String", "string" },
            { "System.Object", "object" },
            { "System.Boolean", "bool" },
            { "System.Void", "void" },
            { "System.Char", "char" },
            { "System.Byte", "byte" },
            { "System.UInt16", "ushort" },
            { "System.UInt32", "uint" },
            { "System.UInt64", "ulong" },
            { "System.SByte", "sbyte" },
            { "System.Single", "float" },
            { "System.Double", "double" },
            { "System.Decimal", "decimal" },
        };
    }
}
