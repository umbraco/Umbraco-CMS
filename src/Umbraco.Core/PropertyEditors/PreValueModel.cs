using System;
using System.Collections.Generic;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Abstract class representing a Property Editor's model to render it's Pre value editor
    /// </summary>
    internal abstract class PreValueModel
    {
        protected virtual IEnumerable<PreValueDefinition> GetValueDefinitions()
        {
            throw new NotImplementedException();
        }

        public virtual string GetSerializedValue()
        {
            throw new NotImplementedException();
        }

        protected virtual void SetModelPropertyValue(PreValueDefinition def, Action<object> setProperty)
        {
            throw new NotImplementedException();
        }

        public virtual void SetModelValues(string serializedVal)
        {
            throw new NotImplementedException();
        }
    }
}