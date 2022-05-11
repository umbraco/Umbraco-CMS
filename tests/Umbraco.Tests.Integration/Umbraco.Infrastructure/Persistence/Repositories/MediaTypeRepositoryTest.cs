// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MediaTypeRepositoryTest : UmbracoIntegrationTest
    {
        private IContentTypeCommonRepository CommonRepository => GetRequiredService<IContentTypeCommonRepository>();

        private ILanguageRepository LanguageRepository => GetRequiredService<ILanguageRepository>();

        [Test]
        public void Can_Move()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                EntityContainerRepository containerRepository = CreateContainerRepository(provider);
                MediaTypeRepository repository = CreateRepository(provider);

                var container1 = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah1" };
                containerRepository.Save(container1);

                var container2 = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah2", ParentId = container1.Id };
                containerRepository.Save(container2);

                IMediaType contentType =
                    MediaTypeBuilder.CreateNewMediaType();
                contentType.ParentId = container2.Id;
                repository.Save(contentType);

                // create a
                var contentType2 = (IMediaType)new MediaType(ShortStringHelper, contentType, "hello")
                {
                    Name = "Blahasdfsadf"
                };
                contentType.ParentId = contentType.Id;
                repository.Save(contentType2);

                global::Umbraco.Cms.Core.Events.MoveEventInfo<IMediaType>[] result = repository.Move(contentType, container1).ToArray();

                Assert.AreEqual(2, result.Length);

                // re-get
                contentType = repository.Get(contentType.Id);
                contentType2 = repository.Get(contentType2.Id);

                scope.Rollback();

                Assert.AreEqual(container1.Id, contentType.ParentId);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType.Id).OriginalPath, contentType.Path);
                Assert.AreNotEqual(result.Single(x => x.Entity.Id == contentType2.Id).OriginalPath, contentType2.Path);
            }
        }

        [Test]
        public void Can_Create_Container()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                EntityContainerRepository containerRepository = CreateContainerRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);

                Assert.That(container.Id, Is.GreaterThan(0));

                EntityContainer found = containerRepository.Get(container.Id);
                Assert.IsNotNull(found);

                scope.Rollback();
            }
        }

        [Test]
        public void Can_Delete_Container()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                EntityContainerRepository containerRepository = CreateContainerRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);

                Assert.That(container.Id, Is.GreaterThan(0));

                // Act
                containerRepository.Delete(container);

                EntityContainer found = containerRepository.Get(container.Id);
                Assert.IsNull(found);

                scope.Rollback();
            }
        }

        [Test]
        public void Can_Create_Container_Containing_Media_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                EntityContainerRepository containerRepository = CreateContainerRepository(provider);
                MediaTypeRepository repository = CreateRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);

                MediaType contentType =
                    MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
                contentType.ParentId = container.Id;
                repository.Save(contentType);

                scope.Rollback();

                Assert.AreEqual(container.Id, contentType.ParentId);
            }
        }

        [Test]
        public void Can_Delete_Container_Containing_Media_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                EntityContainerRepository containerRepository = CreateContainerRepository(provider);
                MediaTypeRepository repository = CreateRepository(provider);

                var container = new EntityContainer(Constants.ObjectTypes.MediaType) { Name = "blah" };
                containerRepository.Save(container);

                IMediaType contentType =
                    MediaTypeBuilder.CreateSimpleMediaType("test", "Test", propertyGroupAlias: "testGroup", propertyGroupName: "testGroup");
                contentType.ParentId = container.Id;
                repository.Save(contentType);

                // Act
                containerRepository.Delete(container);

                EntityContainer found = containerRepository.Get(container.Id);
                Assert.IsNull(found);

                contentType = repository.Get(contentType.Id);
                Assert.IsNotNull(contentType);
                Assert.AreEqual(-1, contentType.ParentId);

                scope.Rollback();
            }
        }

        [Test]
        public void Can_Perform_Add_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                MediaType contentType = MediaTypeBuilder.CreateNewMediaType();
                repository.Save(contentType);

                IMediaType fetched = repository.Get(contentType.Id);

                scope.Rollback();

                // Assert
                Assert.That(contentType.HasIdentity, Is.True);
                Assert.That(contentType.PropertyGroups.All(x => x.HasIdentity), Is.True);
                Assert.That(contentType.Path.Contains(","), Is.True);
                Assert.That(contentType.SortOrder, Is.GreaterThan(0));

                TestHelper.AssertPropertyValuesAreEqual(contentType, fetched, ignoreProperties: new[] { "UpdateDate" });
            }
        }

        [Test]
        public void Can_Perform_Update_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaTypeRepository repository = CreateRepository(provider);

                MediaType videoMediaType = MediaTypeBuilder.CreateNewMediaType();
                repository.Save(videoMediaType);

                // Act
                IMediaType mediaType = repository.Get(videoMediaType.Id);

                mediaType.Thumbnail = "Doc2.png";
                mediaType.PropertyGroups["media"].PropertyTypes.Add(new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "subtitle")
                    {
                        Name = "Subtitle",
                        Description = "Optional Subtitle",
                        Mandatory = false,
                        SortOrder = 1,
                        DataTypeId = -88
                    });
                repository.Save(mediaType);

                scope.Rollback();

                bool dirty = ((MediaType)mediaType).IsDirty();

                // Assert
                Assert.That(mediaType.HasIdentity, Is.True);
                Assert.That(dirty, Is.False);
                Assert.That(mediaType.Thumbnail, Is.EqualTo("Doc2.png"));
                Assert.That(mediaType.PropertyTypes.Any(x => x.Alias == "subtitle"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Delete_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                MediaType mediaType = MediaTypeBuilder.CreateNewMediaType();
                repository.Save(mediaType);

                IMediaType contentType2 = repository.Get(mediaType.Id);
                repository.Delete(contentType2);

                bool exists = repository.Exists(mediaType.Id);

                scope.Rollback();

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Get_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                IMediaType mediaType = repository.Get(1033); // File

                // Assert
                Assert.That(mediaType, Is.Not.Null);
                Assert.That(mediaType.Id, Is.EqualTo(1033));
                Assert.That(mediaType.Name, Is.EqualTo(Constants.Conventions.MediaTypes.File));
            }
        }

        [Test]
        public void Can_Perform_Get_By_Guid_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                IMediaType mediaType = repository.Get(1033); // File

                // Act
                mediaType = repository.Get(mediaType.Key);

                // Assert
                Assert.That(mediaType, Is.Not.Null);
                Assert.That(mediaType.Id, Is.EqualTo(1033));
                Assert.That(mediaType.Name, Is.EqualTo(Constants.Conventions.MediaTypes.File));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                IEnumerable<IMediaType> mediaTypes = repository.GetMany();
                int count =
                    ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.MediaType });

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Guid_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                Guid[] allGuidIds = repository.GetMany().Select(x => x.Key).ToArray();

                // Act
                IEnumerable<IMediaType> mediaTypes = ((IReadRepository<Guid, IMediaType>)repository).GetMany(allGuidIds);

                int count =
                    ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @NodeObjectType",
                        new { NodeObjectType = Constants.ObjectTypes.MediaType });

                // Assert
                Assert.That(mediaTypes.Any(), Is.True);
                Assert.That(mediaTypes.Count(), Is.EqualTo(count));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_MediaTypeRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                bool exists = repository.Exists(1032); // Image

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Update_MediaType_With_PropertyType_Removed()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaTypeRepository repository = CreateRepository(provider);

                MediaType mediaType = MediaTypeBuilder.CreateNewMediaType();
                repository.Save(mediaType);

                // Act
                IMediaType mediaTypeV2 = repository.Get(mediaType.Id);
                mediaTypeV2.PropertyGroups["media"].PropertyTypes.Remove("title");
                repository.Save(mediaTypeV2);

                IMediaType mediaTypeV3 = repository.Get(mediaType.Id);

                scope.Rollback();

                // Assert
                Assert.That(mediaTypeV3.PropertyTypes.Any(x => x.Alias == "title"), Is.False);
                Assert.That(mediaTypeV2.PropertyGroups.Count, Is.EqualTo(mediaTypeV3.PropertyGroups.Count));
                Assert.That(mediaTypeV2.PropertyTypes.Count(), Is.EqualTo(mediaTypeV3.PropertyTypes.Count()));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_Video_MediaType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MediaTypeRepository repository = CreateRepository(provider);

                MediaType mediaType = MediaTypeBuilder.CreateNewMediaType();

                repository.Save(mediaType);

                // Act
                IMediaType contentType = repository.Get(mediaType.Id);

                scope.Rollback();

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(2));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Verify_PropertyTypes_On_File_MediaType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope(autoComplete: true))
            {
                MediaTypeRepository repository = CreateRepository(provider);

                // Act
                IMediaType contentType = repository.Get(1033); // File

                // Assert
                Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(3));
                Assert.That(contentType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        private MediaTypeRepository CreateRepository(IScopeProvider provider) =>
            new MediaTypeRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<MediaTypeRepository>(), CommonRepository, LanguageRepository, ShortStringHelper);

        private EntityContainerRepository CreateContainerRepository(IScopeProvider provider) =>
            new EntityContainerRepository((IScopeAccessor)provider, AppCaches.Disabled, LoggerFactory.CreateLogger<EntityContainerRepository>(), Constants.ObjectTypes.MediaTypeContainer);
    }
}
