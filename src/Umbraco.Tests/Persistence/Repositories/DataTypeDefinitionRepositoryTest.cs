using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DataTypeDefinitionRepositoryTest : TestWithDatabaseBase
    {
        private IDataTypeRepository CreateRepository()
        {
            return Factory.GetInstance<IDataTypeRepository>();
        }

        private EntityContainerRepository CreateContainerRepository(IScopeAccessor scopeAccessor)
        {
            return new EntityContainerRepository(scopeAccessor, AppCaches.Disabled, Logger, Constants.ObjectTypes.DataTypeContainer);
        }

        [Test]
        public void Can_Find_Usages()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var dtRepo = CreateRepository();
                IDataType dataType1 = new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService)) { Name = "dt1" };
                dtRepo.Save(dataType1);
                IDataType dataType2 = new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService)) { Name = "dt2" };
                dtRepo.Save(dataType2);

                var ctRepo = Factory.GetInstance<IContentTypeRepository>();
                IContentType ct = new ContentType(-1)
                {
                    Alias = "ct1",
                    Name = "CT1",
                    AllowedAsRoot = true,
                    Icon = "icon-home",
                    PropertyGroups = new PropertyGroupCollection
                    {
                        new PropertyGroup(true)
                        {
                            Name = "PG1",
                            PropertyTypes = new PropertyTypeCollection(true)
                            {
                                new PropertyType(dataType1, "pt1")
                                {
                                    Name = "PT1"
                                },
                                new PropertyType(dataType1, "pt2")
                                {
                                    Name = "PT2"
                                },
                                new PropertyType(dataType2, "pt3")
                                {
                                    Name = "PT3"
                                }
                            }
                        }
                    }
                };
                ctRepo.Save(ct);

                var usages = dtRepo.FindUsages(dataType1.Id);

                var key = usages.First().Key;
                Assert.AreEqual(ct.Key, ((GuidUdi)key).Guid);
                Assert.AreEqual(2, usages[key].Count());
                Assert.AreEqual("pt1", usages[key].ElementAt(0));
                Assert.AreEqual("pt2", usages[key].ElementAt(1));

                usages = dtRepo.FindUsages(dataType2.Id);

                key = usages.First().Key;
                Assert.AreEqual(ct.Key, ((GuidUdi)key).Guid);
                Assert.AreEqual(1, usages[key].Count());
                Assert.AreEqual("pt3", usages[key].ElementAt(0));
            }
        }

        [Test]
        public void Can_Move()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var accessor = (IScopeAccessor) provider;

            using (provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(accessor);
                var repository = CreateRepository();
                var container1 = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah1" };
                containerRepository.Save(container1);

                var container2 = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah2", ParentId = container1.Id };
                containerRepository.Save(container2);

                var dataType = (IDataType) new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService), container2.Id)
                {
                    Name = "dt1"
                };
                repository.Save(dataType);

                //create a
                var dataType2 = (IDataType)new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService), dataType.Id)
                {
                    Name = "dt2"
                };
                repository.Save(dataType2);

                var result = repository.Move(dataType, container1).ToArray();

                Assert.AreEqual(2, result.Length);

                //re-get
                dataType = repository.Get(dataType.Id);
                dataType2 = repository.Get(dataType2.Id);

                Assert.AreEqual(container1.Id, dataType.ParentId);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == dataType.Id).OriginalPath, dataType.Path);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == dataType2.Id).OriginalPath, dataType2.Path);
            }
        }

        [Test]
        public void Can_Create_Container()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var accessor = (IScopeAccessor) provider;

            using (provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(accessor);
                var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
                containerRepository.Save(container);

                Assert.That(container.Id, Is.GreaterThan(0));

                var found = containerRepository.Get(container.Id);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var accessor = (IScopeAccessor) provider;

            using (provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(accessor);
                var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
                containerRepository.Save(container);

                // Act
                containerRepository.Delete(container);

                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Data_Types()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var accessor = (IScopeAccessor) provider;

            using (provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(accessor);
                var repository = CreateRepository();
                var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
                containerRepository.Save(container);

                var dataTypeDefinition = new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService), container.Id) { Name = "test" };
                repository.Save(dataTypeDefinition);

                Assert.AreEqual(container.Id, dataTypeDefinition.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Data_Types()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var accessor = (IScopeAccessor) provider;

            using (provider.CreateScope())
            {
                var containerRepository = CreateContainerRepository(accessor);
                var repository = CreateRepository();
                var container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "blah" };
                containerRepository.Save(container);

                IDataType dataType = new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService), container.Id) { Name = "test" };
                repository.Save(dataType);

                // Act
                containerRepository.Delete(container);

                var found = containerRepository.Get(container.Id);
                Assert.IsNull(found);

                dataType = repository.Get(dataType.Id);
                Assert.IsNotNull(dataType);
                Assert.AreEqual(-1, dataType.ParentId);
            }
        }

        [Test]
        public void Can_Create()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();
                IDataType dataType = new DataType(new RadioButtonsPropertyEditor(Logger, ServiceContext.TextService)) {Name = "test"};

                repository.Save(dataType);

                var id = dataType.Id;
                Assert.That(id, Is.GreaterThan(0));

                // Act
                dataType = repository.Get(id);

                // Assert
                Assert.That(dataType, Is.Not.Null);
                Assert.That(dataType.HasIdentity, Is.True);
            }
        }


        [Test]
        public void Can_Perform_Get_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();
                // Act
                var dataTypeDefinition = repository.Get(Constants.DataTypes.DropDownSingle);

                // Assert
                Assert.That(dataTypeDefinition, Is.Not.Null);
                Assert.That(dataTypeDefinition.HasIdentity, Is.True);
                Assert.That(dataTypeDefinition.Name, Is.EqualTo("Dropdown"));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();

                // Act
                var dataTypeDefinitions = repository.GetMany().ToArray();

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
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();

                // Act
                var dataTypeDefinitions = repository.GetMany(-40, -41, -42).ToArray();

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
            var provider = TestObjects.GetScopeProvider(Logger);

            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository();

                // Act
                var query = scope.SqlContext.Query<IDataType>().Where(x => x.EditorAlias == Constants.PropertyEditors.Aliases.RadioButtonList);
                var result = repository.Get(query).ToArray();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.FirstOrDefault()?.Name, Is.EqualTo("Radiobox"));
            }
        }

        [Test]
        public void Can_Perform_Count_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository();

                // Act
                var query = scope.SqlContext.Query<IDataType>().Where(x => x.Name.StartsWith("D"));
                int count = repository.Count(query);

                // Assert
                Assert.That(count, Is.EqualTo(4));
            }
        }

        [Test]
        public void Can_Perform_Add_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();
                var dataTypeDefinition = new DataType(new LabelPropertyEditor(Logger))
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0,
                    Configuration = new LabelConfiguration { ValueType = ValueTypes.Xml }
                };

                // Act
                repository.Save(dataTypeDefinition);

                var exists = repository.Exists(dataTypeDefinition.Id);
                var fetched = repository.Get(dataTypeDefinition.Id);

                // Assert
                Assert.That(dataTypeDefinition.HasIdentity, Is.True);
                Assert.That(exists, Is.True);

                // cannot compare 'configuration' as it's two different objects
                TestHelper.AssertPropertyValuesAreEqual(dataTypeDefinition, fetched, "yyyy-MM-dd HH:mm:ss", ignoreProperties: new [] { "Configuration" });

                // still, can compare explicitely
                Assert.IsNotNull(dataTypeDefinition.Configuration);
                Assert.IsInstanceOf<LabelConfiguration>(dataTypeDefinition.Configuration);
                Assert.IsNotNull(fetched.Configuration);
                Assert.IsInstanceOf<LabelConfiguration>(fetched.Configuration);
                Assert.AreEqual(ConfigurationEditor.ConfigurationAs<LabelConfiguration>(dataTypeDefinition.Configuration).ValueType, ConfigurationEditor.ConfigurationAs<LabelConfiguration>(fetched.Configuration).ValueType);
            }
        }

        [Test]
        public void Can_Perform_Update_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();
                var dataTypeDefinition = new DataType(new IntegerPropertyEditor(Logger))
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0
                };
                repository.Save(dataTypeDefinition);

                // Act
                var definition = repository.Get(dataTypeDefinition.Id);
                definition.Name = "AgeDataType Updated";
                definition.Editor = new LabelPropertyEditor(Logger); //change
                repository.Save(definition);

                var definitionUpdated = repository.Get(dataTypeDefinition.Id);

                // Assert
                Assert.That(definitionUpdated, Is.Not.Null);
                Assert.That(definitionUpdated.Name, Is.EqualTo("AgeDataType Updated"));
                Assert.That(definitionUpdated.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Label));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();
                var dataTypeDefinition = new DataType(new LabelPropertyEditor(Logger))
                {
                    DatabaseType = ValueStorageType.Integer,
                    Name = "AgeDataType",
                    CreatorId = 0
                };

                // Act
                repository.Save(dataTypeDefinition);

                var existsBefore = repository.Exists(dataTypeDefinition.Id);

                repository.Delete(dataTypeDefinition);

                var existsAfter = repository.Exists(dataTypeDefinition.Id);

                // Assert
                Assert.That(existsBefore, Is.True);
                Assert.That(existsAfter, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_DataTypeDefinitionRepository()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            using (provider.CreateScope())
            {
                var repository = CreateRepository();

                // Act
                var exists = repository.Exists(1046); //Content picker
                var doesntExist = repository.Exists(-80);

                // Assert
                Assert.That(exists, Is.True);
                Assert.That(doesntExist, Is.False);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}
