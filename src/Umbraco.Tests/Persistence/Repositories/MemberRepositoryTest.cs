using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MemberRepositoryTest : TestWithDatabaseBase
    {
        private MemberRepository CreateRepository(IScopeProvider provider, out MemberTypeRepository memberTypeRepository, out MemberGroupRepository memberGroupRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = Mock.Of<ITemplateRepository>();
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            memberTypeRepository = new MemberTypeRepository(accessor, AppCaches.Disabled, Logger, commonRepository);
            memberGroupRepository = new MemberGroupRepository(accessor, AppCaches.Disabled, Logger);
            var tagRepo = new TagRepository(accessor, AppCaches.Disabled, Logger);
            var repository = new MemberRepository(accessor, AppCaches.Disabled, Logger, memberTypeRepository, memberGroupRepository, tagRepo, Mock.Of<ILanguageRepository>());
            return repository;
        }

        [Test]
        public void GetMember()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var member = CreateTestMember();

                member = repository.Get(member.Id);

                Assert.That(member, Is.Not.Null);
                Assert.That(member.HasIdentity, Is.True);
            }
        }

        [Test]
        public void GetMembers()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var type = CreateTestMemberType();
                var m1 = CreateTestMember(type, "Test 1", "test1@test.com", "pass1", "test1");
                var m2 = CreateTestMember(type, "Test 2", "test2@test.com", "pass2", "test2");

                var members = repository.GetMany(m1.Id, m2.Id);

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Count(), Is.EqualTo(2));
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
            }
        }

        [Test]
        public void GetAllMembers()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var type = CreateTestMemberType();
                for (var i = 0; i < 5; i++)
                {
                    CreateTestMember(type, "Test " + i, "test" + i + "@test.com", "pass" + i, "test" + i);
                }

                var members = repository.GetMany();

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Count(), Is.EqualTo(5));
                Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
            }
        }

        [Test]
        public void QueryMember()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var key = Guid.NewGuid();
                var member = CreateTestMember(key: key);

                // Act
                var query = scope.SqlContext.Query<IMember>().Where(x => x.Key == key);
                var result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(member.Id));
            }
        }

        [Test]
        public void SaveMember()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var member = CreateTestMember();

                var sut = repository.Get(member.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.HasIdentity, Is.True);
                Assert.That(sut.Name, Is.EqualTo("Johnny Hefty"));
                Assert.That(sut.Email, Is.EqualTo("johnny@example.com"));
                Assert.That(sut.RawPasswordValue, Is.EqualTo("123"));
                Assert.That(sut.Username, Is.EqualTo("hefty"));

                TestHelper.AssertPropertyValuesAreEqual(sut, member, "yyyy-MM-dd HH:mm:ss");
            }
        }

        [Test]
        public void MemberHasBuiltinProperties()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.Save(memberType);

                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.Save(member);

                var sut = repository.Get(member.Id);

                Assert.That(memberType.CompositionPropertyGroups.Count(), Is.EqualTo(2));
                Assert.That(memberType.CompositionPropertyTypes.Count(), Is.EqualTo(3 + Constants.Conventions.Member.GetStandardPropertyTypeStubs().Count));
                Assert.That(sut.Properties.Count(), Is.EqualTo(3 + Constants.Conventions.Member.GetStandardPropertyTypeStubs().Count));
                var grp = memberType.CompositionPropertyGroups.FirstOrDefault(x => x.Name == Constants.Conventions.Member.StandardPropertiesGroupName);
                Assert.IsNotNull(grp);
                var aliases = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
                foreach (var p in memberType.CompositionPropertyTypes.Where(x => aliases.Contains(x.Alias)))
                {
                    Assert.AreEqual(grp.Id, p.PropertyGroupId.Value);
                }
            }
        }

        [Test]
        public void SavingPreservesPassword()
        {
            IMember sut;
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.Save(memberType);


                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.Save(member);


                sut = repository.Get(member.Id);
                //when the password is null it will not overwrite what is already there.
                sut.RawPasswordValue = null;
                repository.Save(sut);

                sut = repository.Get(member.Id);

                Assert.That(sut.RawPasswordValue, Is.EqualTo("123"));
            }
        }

        [Test]
        public void SavingUpdatesNameAndEmail()
        {
            IMember sut;
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.Save(memberType);


                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.Save(member);


                sut = repository.Get(member.Id);
                sut.Username = "This is new";
                sut.Email = "thisisnew@hello.com";
                repository.Save(sut);

                sut = repository.Get(member.Id);

                Assert.That(sut.Email, Is.EqualTo("thisisnew@hello.com"));
                Assert.That(sut.Username, Is.EqualTo("This is new"));
            }
        }

        [Test]
        public void QueryMember_WithSubQuery()
        {
            var provider = TestObjects.GetScopeProvider(Logger);

            var query = provider.SqlContext.Query<IMember>().Where(x =>
                        ((Member) x).LongStringPropertyValue.Contains("1095") &&
                        ((Member) x).PropertyTypeAlias == "headshot");

            var sqlSubquery = GetSubquery();
            var translator = new SqlTranslator<IMember>(sqlSubquery, query);
            var subquery = translator.Translate();
            var sql = GetBaseQuery(false)
                .Append("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            Debug.Print(sql.SQL);
            Assert.That(sql.SQL, Is.Not.Empty);
        }

        private IMember CreateTestMember(IMemberType memberType = null, string name = null, string email = null, string password = null, string username = null, Guid? key = null)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                if (memberType == null)
                {
                    memberType = MockedContentTypes.CreateSimpleMemberType();
                    memberTypeRepository.Save(memberType);

                }

                var member = MockedMember.CreateSimpleMember(memberType, name ?? "Johnny Hefty", email ?? "johnny@example.com", password ?? "123", username ?? "hefty", key);
                repository.Save(member);
                scope.Complete();

                return member;
            }
        }

        private IMemberType CreateTestMemberType(string alias = null)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                MemberTypeRepository memberTypeRepository;
                MemberGroupRepository memberGroupRepository;
                var repository = CreateRepository(provider, out memberTypeRepository, out memberGroupRepository);

                var memberType = MockedContentTypes.CreateSimpleMemberType(alias);
                memberTypeRepository.Save(memberType);
                scope.Complete();
                return memberType;
            }
        }

        private Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            if (isCount)
            {
                var sqlCount = provider.SqlContext.Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sqlCount;
            }

            var sql = provider.SqlContext.Sql();
            sql.Select("umbracoNode.*", $"{Constants.DatabaseSchema.Tables.Content}.contentTypeId", "cmsContentType.alias AS ContentTypeAlias", $"{Constants.DatabaseSchema.Tables.ContentVersion}.versionId",
                $"{Constants.DatabaseSchema.Tables.ContentVersion}.versionDate", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password",
                Constants.DatabaseSchema.Tables.PropertyData + ".id AS PropertyDataId", Constants.DatabaseSchema.Tables.PropertyData + ".propertytypeid",
                Constants.DatabaseSchema.Tables.PropertyData + ".dateValue", Constants.DatabaseSchema.Tables.PropertyData + ".intValue",
                Constants.DatabaseSchema.Tables.PropertyData + ".textValue", Constants.DatabaseSchema.Tables.PropertyData + ".varcharValue",
                "cmsPropertyType.id", "cmsPropertyType.Alias", "cmsPropertyType.Description",
                "cmsPropertyType.Name", "cmsPropertyType.mandatory", "cmsPropertyType.validationRegExp",
                "cmsPropertyType.sortOrder AS PropertyTypeSortOrder", "cmsPropertyType.propertyTypeGroupId",
                "cmsPropertyType.dataTypeId", "cmsDataType.propertyEditorAlias", "cmsDataType.dbType")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND " + Constants.DatabaseSchema.Tables.PropertyData + $".versionId = {Constants.DatabaseSchema.Tables.ContentVersion}.id")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        private Sql<ISqlContext> GetSubquery()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            var sql = provider.SqlContext.Sql();
            sql.Select("umbracoNode.id")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND " + Constants.DatabaseSchema.Tables.PropertyData + $".versionId = {Constants.DatabaseSchema.Tables.ContentVersion}.id")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        private Guid NodeObjectTypeId => Constants.ObjectTypes.Member;
    }
}
