using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A class representing a single value of a Pre-Value editor to be saved
    /// </summary>
    internal class PreValueDefinition
    {
        public PreValueDefinition(string propertyName, Type modeType, object modelValue)
        {
            ModelType = modeType;
            PropertyName = propertyName;
            ModelValue = modelValue;
        }

        public Type ModelType { get; private set; }
        public string PropertyName { get; private set; }
        public object ModelValue { get; private set; }
    }
}