using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IDataTypeDefinition : IUmbracoEntity
    {
        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Id of the DataType control
        /// </summary>
        Guid ControlId { get; }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        DataTypeDatabaseType DatabaseType { get; set; }
    }
}