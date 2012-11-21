using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    public interface IDataTypeDefinition : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        [DataMember]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        int Level { get; set; }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        string Path { get; set; }

        /// <summary>
        /// Id of the user who created this entity
        /// </summary>
        [DataMember]
        IProfile Creator { get; set; }

        /// <summary>
        /// Boolean indicating whether this entity is Trashed or not.
        /// </summary>
        [DataMember]
        bool Trashed { get; }

        /// <summary>
        /// Id of the DataType control
        /// </summary>
        [DataMember]
        Guid ControlId { get; }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        [DataMember]
        DataTypeDatabaseType DatabaseType { get; set; }
    }
}