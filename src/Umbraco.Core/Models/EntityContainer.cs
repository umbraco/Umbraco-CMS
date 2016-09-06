﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a folder for organizing entities such as content types and data types.
    /// </summary>
    public sealed class EntityContainer : UmbracoEntity
    {
        private readonly Guid _containedObjectType;

        private static readonly Dictionary<Guid, Guid> ObjectTypeMap = new Dictionary<Guid, Guid>
        {
            { Constants.ObjectTypes.DataTypeGuid, Constants.ObjectTypes.DataTypeContainerGuid },
            { Constants.ObjectTypes.DocumentTypeGuid, Constants.ObjectTypes.DocumentTypeContainerGuid },
            { Constants.ObjectTypes.MediaTypeGuid, Constants.ObjectTypes.MediaTypeContainerGuid }
        };

        /// <summary>
        /// Initializes a new instance of an <see cref="EntityContainer"/> class.
        /// </summary>
        public EntityContainer(Guid containedObjectType)
        {
            if (ObjectTypeMap.ContainsKey(containedObjectType) == false)
                throw new ArgumentException("Not a contained object type.", "containedObjectType");
            _containedObjectType = containedObjectType;

            ParentId = -1;
            Path = "-1";
            Level = 0;
            SortOrder = 0;
        }

        /// <summary>
        /// Initializes a new instance of an <see cref="EntityContainer"/> class.
        /// </summary>
        internal EntityContainer(int id, Guid uniqueId, int parentId, string path, int level, int sortOrder, Guid containedObjectType, string name, int userId)
            : this(containedObjectType)
        {
            Id = id;
            Key = uniqueId;
            ParentId = parentId;
            Name = name;
            Path = path;
            Level = level;
            SortOrder = sortOrder;
            CreatorId = userId;
        }

        /// <summary>
        /// Gets or sets the node object type of the contained objects.
        /// </summary>
        public Guid ContainedObjectType
        {
            get { return _containedObjectType; }
        }

        /// <summary>
        /// Gets the node object type of the container objects.
        /// </summary>
        public Guid ContainerObjectType
        {
            get { return ObjectTypeMap[_containedObjectType]; }
        }

        /// <summary>
        /// Gets the container object type corresponding to a contained object type.
        /// </summary>
        /// <param name="containedObjectType">The contained object type.</param>
        /// <returns>The object type of containers containing objects of the contained object type.</returns>
        public static Guid GetContainerObjectType(Guid containedObjectType)
        {
            if (ObjectTypeMap.ContainsKey(containedObjectType) == false)
                throw new ArgumentException("Not a contained object type.", "containedObjectType");
            return ObjectTypeMap[containedObjectType];
        }

        /// <summary>
        /// Gets the contained object type corresponding to a container object type.
        /// </summary>
        /// <param name="containerObjectType">The container object type.</param>
        /// <returns>The object type of objects that containers of the container object type can contain.</returns>
        public static Guid GetContainedObjectType(Guid containerObjectType)
        {
            var contained = ObjectTypeMap.FirstOrDefault(x => x.Value == containerObjectType).Key;
            if (contained == null)
                throw new ArgumentException("Not a container object type.", "containerObjectType");
            return contained;
        }
    }
}