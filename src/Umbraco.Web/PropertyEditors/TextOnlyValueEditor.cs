using System;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Custom value editor which ensures that the value stored is just plain text and that 
    /// no magic json formatting occurs when translating it to and from the database values
    /// </summary>
    public class TextOnlyValueEditor : PropertyValueEditorWrapper
    {
        public TextOnlyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// A method used to format the database value to a value that can be used by the editor
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        /// <remarks>
        /// The object returned will always be a string and if the database type is not a valid string type an exception is thrown
        /// </remarks>
        public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null) return string.Empty;

            switch (GetDatabaseType())
            {
                case DataTypeDatabaseType.Ntext:
                case DataTypeDatabaseType.Nvarchar:                    
                    return property.Value.ToString();
                case DataTypeDatabaseType.Integer:
                case DataTypeDatabaseType.Decimal:                   
                case DataTypeDatabaseType.Date:                    
                default:
                    throw new InvalidOperationException("The " + typeof(TextOnlyValueEditor) + " can only be used with string based property editors");
            }
        }

    }
}