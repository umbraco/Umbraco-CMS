using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Core.PropertyEditors;
using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Serialization;
using MockedUser = Umbraco.Tests.TestHelpers.Entities.MockedUser;

namespace Umbraco.Tests.Persistence.Repositories
{
    // TODO: Move the remaining parts to Integration tests

    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
    public class UserRepositoryTest : TestWithDatabaseBase
    {
        private MediaRepository CreateMediaRepository(IScopeProvider provider, out IMediaTypeRepository mediaTypeRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock(), IOHelper, ShortStringHelper);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches, ShortStringHelper);
            var languageRepository = new LanguageRepository(accessor, AppCaches, Logger, TestObjects.GetGlobalSettings());
            mediaTypeRepository = new MediaTypeRepository(accessor, AppCaches, Mock.Of<ILogger>(), commonRepository, languageRepository, ShortStringHelper);
            var tagRepository = new TagRepository(accessor, AppCaches, Mock.Of<ILogger>());
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var mediaUrlGenerators = new MediaUrlGeneratorCollection(Enumerable.Empty<IMediaUrlGenerator>());
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new MediaRepository(accessor, AppCaches, Mock.Of<ILogger>(), mediaTypeRepository, tagRepository, Mock.Of<ILanguageRepository>(), relationRepository, relationTypeRepository, propertyEditors, mediaUrlGenerators, dataValueReferences, DataTypeService);
            return repository;
        }

        private DocumentRepository CreateContentRepository(IScopeProvider provider, out IContentTypeRepository contentTypeRepository)
        {
            ITemplateRepository tr;
            return CreateContentRepository(provider, out contentTypeRepository, out tr);
        }

        private DocumentRepository CreateContentRepository(IScopeProvider provider, out IContentTypeRepository contentTypeRepository, out ITemplateRepository templateRepository)
        {
            var accessor = (IScopeAccessor) provider;
            templateRepository = new TemplateRepository(accessor, AppCaches, Logger, TestObjects.GetFileSystemsMock(), IOHelper, ShortStringHelper);
            var tagRepository = new TagRepository(accessor, AppCaches, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches, ShortStringHelper);
            var languageRepository = new LanguageRepository(accessor, AppCaches, Logger, TestObjects.GetGlobalSettings());
            contentTypeRepository = new ContentTypeRepository(accessor, AppCaches, Logger, commonRepository, languageRepository, ShortStringHelper);
            var relationTypeRepository = new RelationTypeRepository(accessor, AppCaches.Disabled, Logger);
            var entityRepository = new EntityRepository(accessor);
            var relationRepository = new RelationRepository(accessor, Logger, relationTypeRepository, entityRepository);
            var propertyEditors = new Lazy<PropertyEditorCollection>(() => new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>())));
            var dataValueReferences = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());
            var repository = new DocumentRepository(accessor, AppCaches, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository, relationRepository, relationTypeRepository, propertyEditors, dataValueReferences, DataTypeService);
            return repository;
        }

        private UserRepository CreateRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            var repository = new UserRepository(accessor, AppCaches.Disabled, Logger, Mappers, TestObjects.GetGlobalSettings(), Mock.Of<IUserPasswordConfiguration>(), new JsonNetSerializer());
            return repository;
        }

        private UserGroupRepository CreateUserGroupRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor) provider;
            return new UserGroupRepository(accessor, AppCaches.Disabled, Logger, ShortStringHelper);
        }

        [Test]
        public void Can_Perform_Update_On_UserRepository()
        {
            var ct = MockedContentTypes.CreateBasicContentType("test");
            var mt = MockedContentTypes.CreateSimpleMediaType("testmedia", "TestMedia");

            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var userRepository = CreateRepository(provider);
                var contentRepository = CreateContentRepository(provider, out var contentTypeRepo);
                var mediaRepository = CreateMediaRepository(provider, out var mediaTypeRepo);
                var userGroupRepository = CreateUserGroupRepository(provider);

                contentTypeRepo.Save(ct);
                mediaTypeRepo.Save(mt);

                var content = MockedContent.CreateBasicContent(ct);
                var media = MockedMedia.CreateSimpleMedia(mt, "asdf", -1);

                contentRepository.Save(content);
                mediaRepository.Save(media);

                var user = CreateAndCommitUserWithGroup(userRepository, userGroupRepository);

                // Act
                var resolved = (User) userRepository.Get(user.Id);

                resolved.Name = "New Name";
                //the db column is not used, default permissions are taken from the user type's permissions, this is a getter only
                //resolved.DefaultPermissions = "ZYX";
                resolved.Language = "fr";
                resolved.IsApproved = false;
                resolved.RawPasswordValue = "new";
                resolved.IsLockedOut = true;
                resolved.StartContentIds = new[] { content.Id };
                resolved.StartMediaIds = new[] { media.Id };
                resolved.Email = "new@new.com";
                resolved.Username = "newName";

                userRepository.Save(resolved);

                var updatedItem = (User) userRepository.Get(user.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                Assert.That(updatedItem.Language, Is.EqualTo(resolved.Language));
                Assert.That(updatedItem.IsApproved, Is.EqualTo(resolved.IsApproved));
                Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(resolved.RawPasswordValue));
                Assert.That(updatedItem.IsLockedOut, Is.EqualTo(resolved.IsLockedOut));
                Assert.IsTrue(updatedItem.StartContentIds.UnsortedSequenceEqual(resolved.StartContentIds));
                Assert.IsTrue(updatedItem.StartMediaIds.UnsortedSequenceEqual(resolved.StartMediaIds));
                Assert.That(updatedItem.Email, Is.EqualTo(resolved.Email));
                Assert.That(updatedItem.Username, Is.EqualTo(resolved.Username));
                Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(resolved.AllowedSections.Count()));
                foreach (var allowedSection in resolved.AllowedSections)
                    Assert.IsTrue(updatedItem.AllowedSections.Contains(allowedSection));
            }
        }


        private static User CreateAndCommitUserWithGroup(IUserRepository repository, IUserGroupRepository userGroupRepository)
        {
            var user = MockedUser.CreateUser();
            repository.Save(user);


            var group = MockedUserGroup.CreateUserGroup();
            userGroupRepository.AddOrUpdateGroupWithUsers(@group, new[] { user.Id });

            user.AddGroup(group);

            return user;
        }

    }
}
