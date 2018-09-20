﻿using System;

namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    /// <summary>
    /// Attribute that represents an Index
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(IndexTypes indexType)
        {
            IndexType = indexType;
        }

        /// <summary>
        /// Gets or sets the name of the Index
        /// </summary>
        /// <remarks>
        /// Overrides default naming of indexes:
        /// IX_tableName
        /// </remarks>
        public string Name { get; set; }//Overrides default naming of indexes: IX_tableName

        /// <summary>
        /// Gets or sets the type of index to create
        /// </summary>
        public IndexTypes IndexType { get; private set; }

        /// <summary>
        /// Gets or sets the column name(s) for the current index
        /// </summary>
        public string ForColumns { get; set; }
    }
}
