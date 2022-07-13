using System.Globalization;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class AddTypedLabels : MigrationBase
{
    public AddTypedLabels(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // insert other label datatypes
        void InsertNodeDto(int id, int sortOrder, string uniqueId, string text)
        {
            var nodeDto = new NodeDto
            {
                NodeId = id,
                Trashed = false,
                ParentId = -1,
                UserId = -1,
                Level = 1,
                Path = "-1,-" + id,
                SortOrder = sortOrder,
                UniqueId = new Guid(uniqueId),
                Text = text,
                NodeObjectType = Constants.ObjectTypes.DataType,
                CreateDate = DateTime.Now,
            };

            Database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, nodeDto);
        }

        if (SqlSyntax.SupportsIdentityInsert())
        {
            Database.Execute(new Sql(
                $"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Node)} ON "));
        }

        InsertNodeDto(Constants.DataTypes.LabelInt, 36, "8e7f995c-bd81-4627-9932-c40e568ec788", "Label (integer)");
        InsertNodeDto(Constants.DataTypes.LabelBigint, 36, "930861bf-e262-4ead-a704-f99453565708", "Label (bigint)");
        InsertNodeDto(Constants.DataTypes.LabelDateTime, 37, "0e9794eb-f9b5-4f20-a788-93acd233a7e4",
            "Label (datetime)");
        InsertNodeDto(Constants.DataTypes.LabelTime, 38, "a97cec69-9b71-4c30-8b12-ec398860d7e8", "Label (time)");
        InsertNodeDto(Constants.DataTypes.LabelDecimal, 39, "8f1ef1e1-9de4-40d3-a072-6673f631ca64", "Label (decimal)");

        if (SqlSyntax.SupportsIdentityInsert())
        {
            Database.Execute(new Sql(
                $"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Node)} OFF "));
        }

        void InsertDataTypeDto(int id, string dbType, string? configuration = null)
        {
            var dataTypeDto = new DataTypeDto
            {
                NodeId = id,
                EditorAlias = Constants.PropertyEditors.Aliases.Label,
                DbType = dbType,
            };

            if (configuration != null)
            {
                dataTypeDto.Configuration = configuration;
            }

            Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
        }

        InsertDataTypeDto(Constants.DataTypes.LabelInt, "Integer", "{\"umbracoDataValueType\":\"INT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelBigint, "Nvarchar", "{\"umbracoDataValueType\":\"BIGINT\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDateTime, "Date", "{\"umbracoDataValueType\":\"DATETIME\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelDecimal, "Decimal", "{\"umbracoDataValueType\":\"DECIMAL\"}");
        InsertDataTypeDto(Constants.DataTypes.LabelTime, "Date", "{\"umbracoDataValueType\":\"TIME\"}");

        // flip known property types
        List<PropertyTypeDto>? labelPropertyTypes = Database.Fetch<PropertyTypeDto>(Sql()
            .Select<PropertyTypeDto>(x => x.Id, x => x.Alias)
            .From<PropertyTypeDto>()
            .Where<PropertyTypeDto>(x => x.DataTypeId == Constants.DataTypes.LabelString));

        var intPropertyAliases = new[]
        {
            Constants.Conventions.Media.Width, Constants.Conventions.Media.Height,
            Constants.Conventions.Member.FailedPasswordAttempts,
        };
        var bigintPropertyAliases = new[] { Constants.Conventions.Media.Bytes };
        var dtPropertyAliases = new[]
        {
            Constants.Conventions.Member.LastLockoutDate, Constants.Conventions.Member.LastLoginDate,
            Constants.Conventions.Member.LastPasswordChangeDate,
        };

        var intPropertyTypes = labelPropertyTypes.Where(pt => intPropertyAliases.Contains(pt.Alias)).Select(pt => pt.Id)
            .ToArray();
        var bigintPropertyTypes = labelPropertyTypes.Where(pt => bigintPropertyAliases.Contains(pt.Alias))
            .Select(pt => pt.Id).ToArray();
        var dtPropertyTypes = labelPropertyTypes.Where(pt => dtPropertyAliases.Contains(pt.Alias)).Select(pt => pt.Id)
            .ToArray();

        Database.Execute(Sql().Update<PropertyTypeDto>(u => u.Set(x => x.DataTypeId, Constants.DataTypes.LabelInt))
            .WhereIn<PropertyTypeDto>(x => x.Id, intPropertyTypes));
        Database.Execute(Sql().Update<PropertyTypeDto>(u => u.Set(x => x.DataTypeId, Constants.DataTypes.LabelInt))
            .WhereIn<PropertyTypeDto>(x => x.Id, intPropertyTypes));
        Database.Execute(Sql().Update<PropertyTypeDto>(u => u.Set(x => x.DataTypeId, Constants.DataTypes.LabelBigint))
            .WhereIn<PropertyTypeDto>(x => x.Id, bigintPropertyTypes));
        Database.Execute(Sql().Update<PropertyTypeDto>(u => u.Set(x => x.DataTypeId, Constants.DataTypes.LabelDateTime))
            .WhereIn<PropertyTypeDto>(x => x.Id, dtPropertyTypes));

        // update values for known property types
        // depending on the size of the site, that *may* take time
        // but we want to parse in C# not in the database
        List<PropertyDataValue>? values = Database.Fetch<PropertyDataValue>(Sql()
            .Select<PropertyDataDto>(x => x.Id, x => x.VarcharValue)
            .From<PropertyDataDto>()
            .WhereIn<PropertyDataDto>(x => x.PropertyTypeId, intPropertyTypes));
        foreach (PropertyDataValue? value in values)
        {
            Database.Execute(Sql()
                .Update<PropertyDataDto>(u => u
                    .Set(
                        x => x.IntegerValue,
                        string.IsNullOrWhiteSpace(value.VarcharValue)
                            ? null
                            : int.Parse(value.VarcharValue, NumberStyles.Any, CultureInfo.InvariantCulture))
                    .Set(x => x.TextValue, null)
                    .Set(x => x.VarcharValue, null))
                .Where<PropertyDataDto>(x => x.Id == value.Id));
        }

        values = Database.Fetch<PropertyDataValue>(Sql().Select<PropertyDataDto>(x => x.Id, x => x.VarcharValue)
            .From<PropertyDataDto>().WhereIn<PropertyDataDto>(x => x.PropertyTypeId, dtPropertyTypes));
        foreach (PropertyDataValue? value in values)
        {
            Database.Execute(Sql()
                .Update<PropertyDataDto>(u => u
                    .Set(x => x.DateValue,
                        string.IsNullOrWhiteSpace(value.VarcharValue)
                            ? null
                            : DateTime.Parse(value.VarcharValue,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None))
                    .Set(x => x.TextValue, null)
                    .Set(x => x.VarcharValue, null))
                .Where<PropertyDataDto>(x => x.Id == value.Id));
        }

        // anything that's custom... ppl will have to figure it out manually, there isn't much we can do about it
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private class PropertyDataValue
    {
        public int Id { get; set; }

        public string? VarcharValue { get; set; }
    }

    // ReSharper restore UnusedAutoPropertyAccessor.Local
}
