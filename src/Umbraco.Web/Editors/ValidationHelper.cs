using System;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Editors
{
    internal class ValidationHelper
    {
        /// <summary>
        /// This will check if any properties of the model are attributed with the RequiredForPersistenceAttribute attribute and if they are it will 
        /// check if that property validates, if it doesn't it means that the current model cannot be persisted because it doesn't have the necessary information
        /// to be saved.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is normally used for things like content creating when the name is empty since we cannot actually create a content item when the name is empty.
        /// This is similar but different from the standard Required validator since we still allow content to be saved when validation fails but there are some 
        /// content fields that are absolutely mandatory for creating/saving.
        /// </remarks>
        internal static bool ModelHasRequiredForPersistenceErrors(object model)
        {
            var requiredForPersistenceProperties = TypeDescriptor.GetProperties(model).Cast<PropertyDescriptor>()
                                                                 .Where(x => x.Attributes.Cast<Attribute>().Any(a => a is RequiredForPersistenceAttribute));

            var validator = new RequiredForPersistenceAttribute();
            return requiredForPersistenceProperties.Any(p => !validator.IsValid(p.GetValue(model)));
        }
    }
}