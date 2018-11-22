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

        public string SourcePropertyName { get; private set; }
        public Type Type { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
    }
}