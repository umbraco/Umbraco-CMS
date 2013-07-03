using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IDataTypeDefinition : IUmbracoEntity
    {
        /// <summary>
        /// Id of the DataType control
        /// </summary>
        Guid ControlId { get; set; }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        DataTypeDatabaseType DatabaseType { get; set; }
    }
}