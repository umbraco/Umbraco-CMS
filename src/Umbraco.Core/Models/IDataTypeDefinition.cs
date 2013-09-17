using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IDataTypeDefinition : IUmbracoEntity
    {
        /// <summary>
        /// The Property editor alias assigned to the data type
        /// </summary>
        string PropertyEditorAlias { get; set; }

        /// <summary>
        /// Id of the DataType control
        /// </summary>
        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the PropertyEditorAlias property instead")]
        Guid ControlId { get; set; }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        DataTypeDatabaseType DatabaseType { get; set; }
    }
}