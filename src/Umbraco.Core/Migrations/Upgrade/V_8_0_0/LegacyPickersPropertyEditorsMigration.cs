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

            var refreshCache = Migrate(GetDataTypes(Constants.PropertyEditors.Legacy.Aliases.ContentPicker));
            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.MediaPicker));
            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.MultipleMediaPicker));
            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.MemberPicker));
            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.MultiNodeTreePicker));
            
            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool Migrate(IEnumerable<DataTypeDto> dataTypes)
        {
            var refreshCache = false;

            foreach (var dataType in dataTypes)
            {
                Context.Logger.Info<LegacyPickersPropertyEditorsMigration>("Migrating " + dataType.EditorAlias + ", " + dataType.NodeId);

                dataType.DbType = ValueStorageType.Ntext.ToString();
                Database.Update(dataType);

                // get property data dtos
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                    .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                // update dtos
                var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, true));

                // persist changes
                foreach (var propertyDataDto in updatedDtos)
                    Database.Update(propertyDataDto);

                refreshCache = true;
            }

            return refreshCache;
        }

        private bool UpdatePropertyDataDto(PropertyDataDto propData, bool isMultiple)
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
                    continue;
                }
                canConvert = false;
            }

            if (!canConvert) return false;

            propData.TextValue = String.Join(",", values);
            propData.VarcharValue = null;
            propData.IntegerValue = null;
            return true;
        }



    }
}
