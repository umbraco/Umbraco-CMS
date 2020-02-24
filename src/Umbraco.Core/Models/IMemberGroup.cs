﻿using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    public interface IMemberGroup : IEntity, IRememberBeingDirty, IHaveAdditionalData
    {
        /// <summary>
        /// The name of the member group
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Profile of the user who created this Entity
        /// </summary>
        int CreatorId { get; set; }
    }
}
