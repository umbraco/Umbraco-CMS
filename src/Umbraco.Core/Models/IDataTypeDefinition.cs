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
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        DataTypeDatabaseType DatabaseType { get; set; }
    }
}