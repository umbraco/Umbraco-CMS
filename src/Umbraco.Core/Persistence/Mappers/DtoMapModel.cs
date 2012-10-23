using System;
using System.Reflection;

namespace Umbraco.Core.Persistence.Mappers
{
    internal class DtoMapModel
    {
        public DtoMapModel(Type type, PropertyInfo propertyInfo, string sourcePropertyName)
        {
            Type = type;
            PropertyInfo = propertyInfo;
            SourcePropertyName = sourcePropertyName;
        }

        public string SourcePropertyName { get; set; }
        public Type Type { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}