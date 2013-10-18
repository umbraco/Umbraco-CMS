using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Caching
{
    [TestFixture]
    public class RuntimeCacheProviderTest
    {
        private IRepositoryCacheProvider _registry;

        [SetUp]
        public void Initiate_Registry()
        {
            _registry = RuntimeCacheProvider.Current;

            //Fill the registry with random entities
            var entity1 = new MockedEntity { Id = 1, Key = 1.ToGuid(), Alias = "mocked1", Name = "Mocked1", Value = Guid.NewGuid().ToString("n") };
            var entity2 = new MockedEntity { Id = 2, Key = 2.ToGuid(), Alias = "mocked2", Name = "Mocked2", Value = Guid.NewGuid().ToString("n") };
            var entity3 = new MockedEntity { Id = 3, Key = 3.ToGuid(), Alias = "mocked3", Name = "Mocked3", Value = Guid.NewGuid().ToString("n") };
            var entity4 = new MockedEntity { Id = 4, Key = 4.ToGuid(), Alias = "mocked4", Name = "Mocked4", Value = Guid.NewGuid().ToString("n") };
            var entity5 = new MockedEntity { Id = 5, Key = 5.ToGuid(), Alias = "mocked5", Name = "Mocked5", Value = Guid.NewGuid().ToString("n") };
            var entity6 = new MockedEntity { Id = 6, Key = 6.ToGuid(), Alias = "mocked6", Name = "Mocked6", Value = Guid.NewGuid().ToString("n") };

            _registry.Save(typeof(MockedEntity), entity1);
            _registry.Save(typeof(MockedEntity), entity2);
            _registry.Save(typeof(MockedEntity), entity3);
            _registry.Save(typeof(MockedEntity), entity4);
            _registry.Save(typeof(MockedEntity), entity5);
            _registry.Save(typeof(MockedEntity), entity6);
        }

        [Test]
        public void Tracked_Keys_Removed_When_Cache_Removed()
        {
            _registry = RuntimeCacheProvider.Current;

            //Fill the registry with random entities
            var entity1 = new MockedEntity { Id = 1, Key = 1.ToGuid(), Alias = "mocked1", Name = "Mocked1", Value = Guid.NewGuid().ToString("n") };
            var entity2 = new MockedEntity { Id = 2, Key = 2.ToGuid(), Alias = "mocked2", Name = "Mocked2", Value = Guid.NewGuid().ToString("n") };
            var entity3 = new MockedEntity { Id = 3, Key = 3.ToGuid(), Alias = "mocked3", Name = "Mocked3", Value = Guid.NewGuid().ToString("n") };

            _registry.Save(typeof(MockedEntity), entity1);
            _registry.Save(typeof(MockedEntity), entity2);
            _registry.Save(typeof(MockedEntity), entity3);

            //now clear the runtime cache internally
            ((RuntimeCacheProvider)_registry).ClearDataCache();

            Assert.AreEqual(0, _registry.GetAllByType(typeof (MockedEntity)).Count());
        }

        [Test]
        public void Can_Clear_By_Type()
        {
            var customObj1 = new CustomMockedEntity { Id = 5, Key = 5.ToGuid(), Alias = "mocked5", Name = "Mocked5", Value = Guid.NewGuid().ToString("n") };
            var customObj2 = new CustomMockedEntity { Id = 6, Key = 6.ToGuid(), Alias = "mocked6", Name = "Mocked6", Value = Guid.NewGuid().ToString("n") };

            _registry.Save(typeof(CustomMockedEntity), customObj1);
            _registry.Save(typeof(CustomMockedEntity), customObj2);

            Assert.AreEqual(2, _registry.GetAllByType(typeof(CustomMockedEntity)).Count());

            _registry.Clear(typeof(CustomMockedEntity));

            Assert.AreEqual(0, _registry.GetAllByType(typeof(CustomMockedEntity)).Count());
        }

        [Test]
        public void Can_Get_Entity_From_Registry()
        {
            // Arrange
            var mockedEntity = new MockedEntity { Id = 20, Key = 20.ToGuid(), Alias = "getMocked", Name = "GetMocked", Value = "Getting entity by id test" };
            _registry.Save(typeof(MockedEntity), mockedEntity);

            // Act
            var entity = _registry.GetById(mockedEntity.GetType(), mockedEntity.Key);

            // Assert
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Id, Is.EqualTo(mockedEntity.Id));
            Assert.That(entity.GetType(), Is.EqualTo(mockedEntity.GetType()));
        }

        [Test]
        public void Can_Get_Entities_By_Ids_From_Registry()
        {
            // Arrange
            var mockedEntity1 = new MockedEntity { Id = 30, Key = 30.ToGuid(), Alias = "getMocked1", Name = "GetMocked1", Value = "Entity 1 - Getting entity by ids test" };
            var mockedEntity2 = new MockedEntity { Id = 31, Key = 31.ToGuid(), Alias = "getMocked2", Name = "GetMocked2", Value = "Entity 2 - Getting entity by ids test" };
            _registry.Save(typeof(MockedEntity), mockedEntity1);
            _registry.Save(typeof(MockedEntity), mockedEntity2);

            // Act
            var entities = _registry.GetByIds(typeof(MockedEntity), new List<Guid> { mockedEntity1.Key, mockedEntity2.Key }).ToList();

            // Assert
            Assert.That(entities, Is.Not.Null);
            Assert.That(entities.Count(), Is.EqualTo(2));
            Assert.That(entities.Any(x => x.Id == mockedEntity1.Id), Is.True);
            Assert.That(entities.Any(x => x.Id == mockedEntity2.Id), Is.True);
        }

        [Test]
        public void Can_Get_Entities_By_Type_From_Registry()
        {
            var entities = _registry.GetAllByType(typeof(MockedEntity));

            Assert.That(entities, Is.Not.Null);
            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void Can_Delete_Entity_From_Registry()
        {
            // Arrange
            var mockedEntity = new MockedEntity { Id = 40, Key = 40.ToGuid(), Alias = "deleteMocked", Name = "DeleteMocked", Value = "Deleting entity test" };
            _registry.Save(typeof(MockedEntity), mockedEntity);
            var entitiesBeforeDeletion = _registry.GetAllByType(typeof(MockedEntity));
            int countBefore = entitiesBeforeDeletion.Count();

            // Act
            var entity = _registry.GetById(mockedEntity.GetType(), mockedEntity.Key);
            _registry.Delete(typeof(MockedEntity), entity);
            var entitiesAfterDeletion = _registry.GetAllByType(typeof(MockedEntity));
            int countAfter = entitiesAfterDeletion.Count();

            // Assert
            Assert.That(countBefore, Is.GreaterThan(countAfter));
            Assert.That(entitiesAfterDeletion.Count(x => x.Id == mockedEntity.Id), Is.EqualTo(0));
        } 
    }
}