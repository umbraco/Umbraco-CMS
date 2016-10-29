﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Definition of a DataType/PropertyEditor
    /// </summary>
    /// <remarks>
    /// The definition exists as a database reference between an actual DataType/PropertyEditor 
    /// (identified by its control id), its prevalues (configuration) and the named DataType in the backoffice UI.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataTypeDefinition : Entity, IDataTypeDefinition
    {
        private int _parentId;
        private string _name;
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _creatorId;
        private bool _trashed;
        private string _propertyEditorAlias;
        private DataTypeDatabaseType _databaseType;

        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the alternative contructor that specifies an alias")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DataTypeDefinition(int parentId, Guid controlId)
        {
            _parentId = parentId;

            _propertyEditorAlias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(controlId, false);
            if (_propertyEditorAlias == null)
            {
                //convert to Label!
                LogHelper.Warn<DataTypeDefinition>("Could not find a GUID -> Alias mapping for the legacy property editor with id " + controlId + ". The DataType has been converted to a Label.");
                _propertyEditorAlias = Constants.PropertyEditors.NoEditAlias;
            }

            _additionalData = new Dictionary<string, object>();
        }

        public DataTypeDefinition(int parentId, string propertyEditorAlias)
        {
            _parentId = parentId;
            _propertyEditorAlias = propertyEditorAlias;

            _additionalData = new Dictionary<string, object>();
        }

        public DataTypeDefinition(string propertyEditorAlias)
        {
            _parentId = -1;
            _propertyEditorAlias = propertyEditorAlias;

            _additionalData = new Dictionary<string, object>();
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, string>(x => x.Name);
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.ParentId);
            public readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.SortOrder);
            public readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.Level);
            public readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, string>(x => x.Path);
            public readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.CreatorId);
            public readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, bool>(x => x.Trashed);
            public readonly PropertyInfo PropertyEditorAliasSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, string>(x => x.PropertyEditorAlias);
            public readonly PropertyInfo DatabaseTypeSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, DataTypeDatabaseType>(x => x.DatabaseType);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector); }
        }

        /// <summary>
        /// Gets or sets the name of the current entity
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetPropertyValueAndDetectChanges(value, ref _sortOrder, Ps.Value.SortOrderSelector); }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public int Level
        {
            get { return _level; }
            set { SetPropertyValueAndDetectChanges(value, ref _level, Ps.Value.LevelSelector); }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        public string Path //Setting this value should be handled by the class not the user
        {
            get { return _path; }
            set { SetPropertyValueAndDetectChanges(value, ref _path, Ps.Value.PathSelector); }
        }

        /// <summary>
        /// Id of the user who created this entity
        /// </summary>
        [DataMember]
        public int CreatorId
        {
            get { return _creatorId; }
            set { SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.UserIdSelector); }
        }

        //NOTE: SD: Why do we have this ??

        /// <summary>
        /// Boolean indicating whether this entity is Trashed or not.
        /// </summary>
        [DataMember]
        public bool Trashed
        {
            get { return _trashed; }
            internal set
            {
                SetPropertyValueAndDetectChanges(value, ref _trashed, Ps.Value.TrashedSelector);
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["Trashed"] = value;
            }
        }
               
        [DataMember]
        public string PropertyEditorAlias
        {
            get { return _propertyEditorAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, Ps.Value.PropertyEditorAliasSelector);
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["DatabaseType"] = value;
            }
        }

        /// <summary>
        /// Id of the DataType control
        /// </summary>
        [DataMember]
        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the PropertyEditorAlias property instead. This method will return a generated GUID for any property editor alias not explicitly mapped to a legacy ID")]
        public Guid ControlId
        {
            get
            {
                return LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(
                    _propertyEditorAlias, LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.GenerateId).Value;
            }
            set
            {
                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(value, true);
                PropertyEditorAlias = alias;
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["ControlId"] = value;
            }
        }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        [DataMember]
        public DataTypeDatabaseType DatabaseType
        {
            get { return _databaseType; }
            set 
            {
                SetPropertyValueAndDetectChanges(value, ref _databaseType, Ps.Value.DatabaseTypeSelector);                
                //This is a custom property that is not exposed in IUmbracoEntity so add it to the additional data
                _additionalData["DatabaseType"] = value;
            }
        }

         private readonly IDictionary<string, object> _additionalData;
        /// <summary>
        /// Some entities may expose additional data that other's might not, this custom data will be available in this collection
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, object> IUmbracoEntity.AdditionalData
        {
            get { return _additionalData; }
        }
    }
}
