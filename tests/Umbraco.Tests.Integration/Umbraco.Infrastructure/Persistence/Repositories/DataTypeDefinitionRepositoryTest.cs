// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DataTypeDefinitionRepositoryTest : UmbracoIntegrationTest
{
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private ILocalizedTextService LocalizedTextService => GetRequiredService<ILocalizedTextService>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

    private IDataTypeContainerRepository DataTypeContainerRepository =>
        GetRequiredService<IDataTypeContainerRepository>();

    private IDataTypeRepository DataTypeRepository => GetRequiredService<IDataTypeRepository>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public void Can_Find_Usages()
    {
        using (ScopeProvider.CreateScope())
        {
            IDataType dataType1 =
                new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer)
                { Name = "dt1" };
            DataTypeRepository.Save(dataType1);
            IDataType dataType2 =
                new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer)
                { Name = "dt2" };
            DataTypeRepository.Save(dataType2);

            IContentType ct = new ContentType(ShortStringHelper, -1)
            {
                Alias = "ct1",
                Name = "CT1",
                AllowedAsRoot = true,
                Icon = "icon-home",
                PropertyGroups = new PropertyGroupCollection
                {
                    new(true)
                    {
                        Name = "PG1",
                        Alias = "pg1",
                        PropertyTypes = new PropertyTypeCollection(true)
                        {
                            new PropertyType(ShortStringHelper, dataType1, "pt1") {Name = "PT1"},
                            new PropertyType(ShortStringHelper, dataType1, "pt2") {Name = "PT2"},
                            new PropertyType(ShortStringHelper, dataType2, "pt3") {Name = "PT3"}
                        }
                    }
                }
            };
            ContentTypeRepository.Save(ct);

            var usages = DataTypeRepository.FindUsages(dataType1.Id);

            var key = usages.First().Key;
            Assert.AreEqual(ct.Key, ((GuidUdi)key).Guid);
            Assert.AreEqual(2, usages[key].Count());
            Assert.AreEqual("pt1", usages[key].ElementAt(0));
            Assert.AreEqual("pt2", usages[key].ElementAt(1));

            usages = DataTypeRepository.FindUsages(dataType2.Id);

            key = usages.First().Key;
            Assert.AreEqual(ct.Key, ((GuidUdi)key).Guid);
            Assert.AreEqual(1, usages[key].Count());
            Assert.AreEqual("pt3", usages[key].ElementAt(0));
        }
    }

    [Test]
    public void Can_Move()
    {
        using (ScopeProvider.CreateScope())
        {
            var container1 = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah1" };
            DataTypeContainerRepository.Save(container1);

            var container2 =
                new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah2", ParentId = container1.Id };
            DataTypeContainerRepository.Save(container2);

            var dataType =
                (IDataType)new DataType(
                    new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer, container2.Id)
                { Name = "dt1" };
            DataTypeRepository.Save(dataType);

            // create a
            var dataType2 =
                (IDataType)new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer, dataType.Id)
                { Name = "dt2" };
            DataTypeRepository.Save(dataType2);

            var result = DataTypeRepository.Move(dataType, container1).ToArray();

            Assert.AreEqual(2, result.Length);

            // re-get
            dataType = DataTypeRepository.Get(dataType.Id);
            dataType2 = DataTypeRepository.Get(dataType2.Id);

            Assert.AreEqual(container1.Id, dataType.ParentId);
            Assert.AreNotEqual(result.Single(x => x.Entity.Id == dataType.Id).OriginalPath, dataType.Path);
            Assert.AreNotEqual(result.Single(x => x.Entity.Id == dataType2.Id).OriginalPath, dataType2.Path);
        }
    }

    [Test]
    public void Can_Create_Container()
    {
        using (ScopeProvider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
            DataTypeContainerRepository.Save(container);

            Assert.That(container.Id, Is.GreaterThan(0));

            var found = DataTypeContainerRepository.Get(container.Id);
            Assert.IsNotNull(found);
        }
    }

    [Test]
    public void Can_Delete_Container()
    {
        using (ScopeProvider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
            DataTypeContainerRepository.Save(container);

            // Act
            DataTypeContainerRepository.Delete(container);

            var found = DataTypeContainerRepository.Get(container.Id);
            Assert.IsNull(found);
        }
    }

    [Test]
    public void Can_Create_Container_Containing_Data_Types()
    {
        using (ScopeProvider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
            DataTypeContainerRepository.Save(container);

            var dataTypeDefinition =
                new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer, container.Id)
                { Name = "test" };
            DataTypeRepository.Save(dataTypeDefinition);

            Assert.AreEqual(container.Id, dataTypeDefinition.ParentId);
        }
    }

    [Test]
    public void Can_Delete_Container_Containing_Data_Types()
    {
        using (ScopeProvider.CreateScope())
        {
            var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
            DataTypeContainerRepository.Save(container);

            IDataType dataType =
                new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer, container.Id)
                { Name = "test" };
            DataTypeRepository.Save(dataType);

            // Act
            DataTypeContainerRepository.Delete(container);

            var found = DataTypeContainerRepository.Get(container.Id);
            Assert.IsNull(found);

            dataType = DataTypeRepository.Get(dataType.Id);
            Assert.IsNotNull(dataType);
            Assert.AreEqual(-1, dataType.ParentId);
        }
    }

    [Test]
    public void Can_Create()
    {
        using (ScopeProvider.CreateScope())
        {
            IDataType dataType =
                new DataType(new RadioButtonsPropertyEditor(DataValueEditorFactory, IOHelper, LocalizedTextService), ConfigurationEditorJsonSerializer)
                { Name = "test" };

            DataTypeRepository.Save(dataType);

            var id = dataType.Id;
            Assert.That(id, Is.GreaterThan(0));

            // Act
            dataType = DataTypeRepository.Get(id);

            // Assert
            Assert.That(dataType, Is.Not.Null);
            Assert.That(dataType.HasIdentity, Is.True);
        }
    }

    [Test]
    public void Can_Perform_Get_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            // Act
            var dataTypeDefinition = DataTypeRepository.Get(Constants.DataTypes.DropDownSingle);

            // Assert
            Assert.That(dataTypeDefinition, Is.Not.Null);
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
            Assert.That(dataTypeDefinition.Name, Is.EqualTo("Dropdown"));
        }
    }

    [Test]
    public void Can_Perform_GetAll_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            // Act
            var dataTypeDefinitions = DataTypeRepository.GetMany().ToArray();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
            Assert.That(dataTypeDefinitions.Length, Is.EqualTo(37));
        }
    }

    [Test]
    public void Can_Perform_GetAll_With_Params_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            // Act
            var dataTypeDefinitions = DataTypeRepository.GetMany(-40, -41, -42).ToArray();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(dataTypeDefinitions.Any(x => x == null), Is.False);
            Assert.That(dataTypeDefinitions.Length, Is.EqualTo(3));
        }
    }

    [Test]
    public void Can_Perform_GetByQuery_On_DataTypeDefinitionRepository()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            // Act
            var query = ScopeProvider.CreateQuery<IDataType>()
                .Where(x => x.EditorAlias == Constants.PropertyEditors.Aliases.RadioButtonList);
            var result = DataTypeRepository.Get(query).ToArray();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
            Assert.That(result.FirstOrDefault()?.Name, Is.EqualTo("Radiobox"));
        }
    }

    [Test]
    public void Can_Perform_Count_On_DataTypeDefinitionRepository()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            // Act
            var query = ScopeProvider.CreateQuery<IDataType>().Where(x => x.Name.StartsWith("D"));
            var count = DataTypeRepository.Count(query);

            // Assert
            Assert.That(count, Is.EqualTo(4));
        }
    }

    [Test]
    public void Can_Perform_Add_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            var dataTypeDefinition =
                new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0,
                    Configuration = new LabelConfiguration { ValueType = ValueTypes.Xml }
                };

            // Act
            DataTypeRepository.Save(dataTypeDefinition);

            var exists = DataTypeRepository.Exists(dataTypeDefinition.Id);
            var fetched = DataTypeRepository.Get(dataTypeDefinition.Id);

            // Assert
            Assert.That(dataTypeDefinition.HasIdentity, Is.True);
            Assert.That(exists, Is.True);

            // cannot compare 'configuration' as it's two different objects
            TestHelper.AssertPropertyValuesAreEqual(dataTypeDefinition, fetched, ignoreProperties: new[] { "Configuration" });

            // still, can compare explicitely
            Assert.IsNotNull(dataTypeDefinition.Configuration);
            Assert.IsInstanceOf<LabelConfiguration>(dataTypeDefinition.Configuration);
            Assert.IsNotNull(fetched.Configuration);
            Assert.IsInstanceOf<LabelConfiguration>(fetched.Configuration);
            Assert.AreEqual(
                ConfigurationEditor.ConfigurationAs<LabelConfiguration>(dataTypeDefinition.Configuration).ValueType,
                ConfigurationEditor.ConfigurationAs<LabelConfiguration>(fetched.Configuration).ValueType);
        }
    }

    [Test]
    public void Can_Perform_Update_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            var dataTypeDefinition =
                new DataType(new IntegerPropertyEditor(DataValueEditorFactory), ConfigurationEditorJsonSerializer)
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0
                };
            DataTypeRepository.Save(dataTypeDefinition);

            // Act
            var definition = DataTypeRepository.Get(dataTypeDefinition.Id);
            definition.Name = "AgeDataType Updated";
            definition.Editor = new LabelPropertyEditor(DataValueEditorFactory, IOHelper); // change
            DataTypeRepository.Save(definition);

            var definitionUpdated = DataTypeRepository.Get(dataTypeDefinition.Id);

            // Assert
            Assert.That(definitionUpdated, Is.Not.Null);
            Assert.That(definitionUpdated.Name, Is.EqualTo("AgeDataType Updated"));
            Assert.That(definitionUpdated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Label));
        }
    }

    [Test]
    public void Can_Perform_Delete_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            var dataTypeDefinition =
                new DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0
                };

            // Act
            DataTypeRepository.Save(dataTypeDefinition);

            var existsBefore = DataTypeRepository.Exists(dataTypeDefinition.Id);

            DataTypeRepository.Delete(dataTypeDefinition);

            var existsAfter = DataTypeRepository.Exists(dataTypeDefinition.Id);

            // Assert
            Assert.That(existsBefore, Is.True);
            Assert.That(existsAfter, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Exists_On_DataTypeDefinitionRepository()
    {
        using (ScopeProvider.CreateScope())
        {
            // Act
            var exists = DataTypeRepository.Exists(1046); // Content picker
            var doesntExist = DataTypeRepository.Exists(-80);

            // Assert
            Assert.That(exists, Is.True);
            Assert.That(doesntExist, Is.False);
        }
    }
}
