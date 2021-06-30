using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models;
using Newtonsoft.Json;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class LegacyPickersPropertyEditorsMigration : PropertyEditorsMigrationBase
    {
        private Lazy<Dictionary<int, NodeDto>> _nodeIdToKey;

        public LegacyPickersPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            _nodeIdToKey = new Lazy<Dictionary<int, NodeDto>>(
                () => Context.Database.Fetch<NodeDto>(
                    Context.SqlContext.Sql()
                        .Select<NodeDto>(x => x.NodeId, x => x.NodeObjectType, x => x.UniqueId)
                        .From<NodeDto>()
                    ).ToDictionary(n => n.NodeId)
                );

            var refreshCache = Migrate(Constants.PropertyEditors.Legacy.Aliases.ContentPicker, ValueStorageType.Nvarchar);
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.MediaPicker, ValueStorageType.Ntext);
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.MultipleMediaPicker, ValueStorageType.Ntext);
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.MemberPicker, ValueStorageType.Nvarchar);
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, ValueStorageType.Ntext);
            
            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool Migrate(string alias, ValueStorageType valueType)
        {
            var refreshCache = false;

            var dataTypes = GetDataTypes(alias);
            foreach (var dataType in dataTypes)
            {
                Context.Logger.Info<LegacyPickersPropertyEditorsMigration>("Migrating " + dataType.EditorAlias + ", " + dataType.NodeId);
                dataType.DbType = valueType.ToString();
                Database.Update(dataType);

                // get property data dtos
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                    .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                // update dtos
                var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, valueType));

                // persist changes
                foreach (var propertyDataDto in updatedDtos)
                    Database.Update(propertyDataDto);

                refreshCache = true;
            }

            return refreshCache;
        }

        private bool UpdatePropertyDataDto(PropertyDataDto propData, ValueStorageType valueType)
        {
            //Get the INT ids stored for this property/drop down
            int[] ids = null;
            if (!propData.VarcharValue.IsNullOrWhiteSpace())
            {
                ids = ConvertStringValues(propData.VarcharValue);
            }
            else if (!propData.TextValue.IsNullOrWhiteSpace())
            {
                ids = ConvertStringValues(propData.TextValue);
            }
            else if (propData.IntegerValue.HasValue)
            {
                ids = new[] { propData.IntegerValue.Value };
            }

            if (ids == null || ids.Length <= 0) return false;

            // map ids to values
            var values = new List<Udi>();
            var canConvert = true;

            foreach (var id in ids)
            {
                if (_nodeIdToKey.Value.TryGetValue(id, out var node))
                {
                    values.Add(Udi.Create(ObjectTypes.GetUdiType(node.NodeObjectType.Value), node.UniqueId));                    
                }
            }

            if (!canConvert) return false;

            propData.IntegerValue = null;
            if (valueType == ValueStorageType.Ntext)
            {
                propData.TextValue = String.Join(",", values);
                propData.VarcharValue = null;
            }
            else if (valueType == ValueStorageType.Nvarchar)
            {
                propData.TextValue = null;
                propData.VarcharValue = String.Join(",", values);
            }
            return true;
        }
    }
}
