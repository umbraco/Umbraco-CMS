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
    public class TextOnlyValueEditor : DataValueEditor
    {
        public TextOnlyValueEditor(DataEditorAttribute attribute)
            : base(attribute)
        { }

        /// <summary>
        /// A method used to format the database value to a value that can be used by the editor
        /// </summary>
        /// <param name="property"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <remarks>
        /// The object returned will always be a string and if the database type is not a valid string type an exception is thrown
        /// </remarks>
        public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
        {
            var val = property.GetValue(culture, segment);

            if (val == null) return string.Empty;

            switch (ValueTypes.ToStorageType(ValueType))
            {
                case ValueStorageType.Ntext:
                case ValueStorageType.Nvarchar:
                    return val.ToString();
                case ValueStorageType.Integer:
                case ValueStorageType.Decimal:
                case ValueStorageType.Date:
                default:
                    throw new InvalidOperationException("The " + typeof(TextOnlyValueEditor) + " can only be used with string based property editors");
            }
        }

    }
}
