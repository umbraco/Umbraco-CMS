// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MemberRepositoryTest : UmbracoIntegrationTest
    {
        private IPasswordHasher PasswordHasher => GetRequiredService<IPasswordHasher>();

        private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

        private IMemberTypeRepository MemberTypeRepository => GetRequiredService<IMemberTypeRepository>();

        private IMemberGroupRepository MemberGroupRepository => GetRequiredService<IMemberGroupRepository>();

        private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

        private MemberRepository CreateRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor)provider;
            ITagRepository tagRepo = GetRequiredService<ITagRepository>();
            IRelationTypeRepository relationTypeRepository = GetRequiredService<IRelationTypeRepository>();
            IRelationRepository relationRepository = GetRequiredService<IRelationRepository>();
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
            var dataValueReferences = new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());
            return new MemberRepository(
                accessor,
                AppCaches.Disabled,
                LoggerFactory.CreateLogger<MemberRepository>(),
                MemberTypeRepository,
                MemberGroupRepository,
                tagRepo,
                Mock.Of<ILanguageRepository>(),
                relationRepository,
                relationTypeRepository,
                PasswordHasher,
                propertyEditors,
                dataValueReferences,
                DataTypeService,
                JsonSerializer,
                Mock.Of<IEventAggregator>(),
                Options.Create(new MemberPasswordConfigurationSettings()));
        }

        [Test]
        public void GetMember()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                IMember member = CreateTestMember();

                member = repository.Get(member.Id);

                Assert.That(member, Is.Not.Null);
                Assert.That(member.HasIdentity, Is.True);
            }
        }

        [Test]
        public void GetMembers()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                IMemberType type = CreateTestMemberType();
                IMember m1 = CreateTestMember(type, "Test 1", "test1@test.com", "pass1", "test1");
                IMember m2 = CreateTestMember(type, "Test 2", "test2@test.com", "pass2", "test2");

                IEnumerable<IMember> members = repository.GetMany(m1.Id, m2.Id);

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Count(), Is.EqualTo(2));
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
            }
        }

        [Test]
        public void GetAllMembers()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                IMemberType type = CreateTestMemberType();
                for (int i = 0; i < 5; i++)
                {
                    CreateTestMember(type, "Test " + i, "test" + i + "@test.com", "pass" + i, "test" + i);
                }

                IEnumerable<IMember> members = repository.GetMany();

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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                var key = Guid.NewGuid();
                IMember member = CreateTestMember(key: key);

                // Act
                IQuery<IMember> query = provider.CreateQuery<IMember>().Where(x => x.Key == key);
                IEnumerable<IMember> result = repository.Get(query);

                // Assert
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(member.Id));
            }
        }

        [Test]
        public void SaveMember()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                IMember member = CreateTestMember();

                IMember sut = repository.Get(member.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.HasIdentity, Is.True);
                Assert.That(sut.Name, Is.EqualTo("Johnny Hefty"));
                Assert.That(sut.Email, Is.EqualTo("johnny@example.com"));
                Assert.That(sut.RawPasswordValue, Is.EqualTo("123"));
                Assert.That(sut.Username, Is.EqualTo("hefty"));

                TestHelper.AssertPropertyValuesAreEqual(sut, member);
            }
        }

        [Test]
        public void MemberHasBuiltinProperties()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                MemberTypeRepository.Save(memberType);

                Member member = MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.Save(member);

                IMember sut = repository.Get(member.Id);

                Assert.That(memberType.CompositionPropertyGroups.Count(), Is.EqualTo(2));
                Assert.That(memberType.CompositionPropertyTypes.Count(), Is.EqualTo(3 + ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Count));
                Assert.That(sut.Properties.Count(), Is.EqualTo(3 + ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Count));
                PropertyGroup grp = memberType.CompositionPropertyGroups.FirstOrDefault(x => x.Name == Constants.Conventions.Member.StandardPropertiesGroupName);
                Assert.IsNotNull(grp);
                string[] aliases = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Select(x => x.Key).ToArray();
                foreach (IPropertyType p in memberType.CompositionPropertyTypes.Where(x => aliases.Contains(x.Alias)))
                {
                    Assert.AreEqual(grp.Id, p.PropertyGroupId.Value);
                }
            }
        }

        [Test]
        public void SavingPreservesPassword()
        {
            IMember sut;
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                MemberTypeRepository.Save(memberType);

                Member member = MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.Save(member);

                sut = repository.Get(member.Id);

                // When the password is null it will not overwrite what is already there.
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
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                MemberTypeRepository.Save(memberType);

                Member member = MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
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
            IScopeProvider provider = ScopeProvider;

            using (provider.CreateScope())
            {
                IQuery<IMember> query = provider.CreateQuery<IMember>().Where(x =>
                    ((Member)x).LongStringPropertyValue.Contains("1095") &&
                    ((Member)x).PropertyTypeAlias == "headshot");

                Sql<ISqlContext> sqlSubquery = GetSubquery();
                var translator = new SqlTranslator<IMember>(sqlSubquery, query);
                Sql<ISqlContext> subquery = translator.Translate();
                Sql<ISqlContext> sql = GetBaseQuery(false)
                    .Append("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments)
                    .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                    .OrderBy<NodeDto>(x => x.SortOrder);

                Debug.Print(sql.SQL);
                Assert.That(sql.SQL, Is.Not.Empty);
            }
        }

        private IMember CreateTestMember(IMemberType memberType = null, string name = null, string email = null, string password = null, string username = null, Guid? key = null)
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                if (memberType == null)
                {
                    memberType = MemberTypeBuilder.CreateSimpleMemberType();
                    MemberTypeRepository.Save(memberType);
                }

                Member member = MemberBuilder.CreateSimpleMember(memberType, name ?? "Johnny Hefty", email ?? "johnny@example.com", password ?? "123", username ?? "hefty", key);
                repository.Save(member);
                scope.Complete();

                return member;
            }
        }

        private IMemberType CreateTestMemberType(string alias = null)
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberRepository repository = CreateRepository(provider);

                MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType(alias);
                MemberTypeRepository.Save(memberType);
                scope.Complete();
                return memberType;
            }
        }

        private Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            IScopeProvider provider = ScopeProvider;
            if (isCount)
            {
                Sql<ISqlContext> sqlCount = ScopeAccessor.AmbientScope.SqlContext.Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sqlCount;
            }

            Sql<ISqlContext> sql = ScopeAccessor.AmbientScope.SqlContext.Sql();
            sql.Select(
                    "umbracoNode.*",
                    $"{Constants.DatabaseSchema.Tables.Content}.contentTypeId",
                    "cmsContentType.alias AS ContentTypeAlias",
                    $"{Constants.DatabaseSchema.Tables.ContentVersion}.versionId",
                    $"{Constants.DatabaseSchema.Tables.ContentVersion}.versionDate",
                    "cmsMember.Email",
                    "cmsMember.LoginName",
                    "cmsMember.Password",
                    Constants.DatabaseSchema.Tables.PropertyData + ".id AS PropertyDataId",
                    Constants.DatabaseSchema.Tables.PropertyData + ".propertytypeid",
                    Constants.DatabaseSchema.Tables.PropertyData + ".dateValue",
                    Constants.DatabaseSchema.Tables.PropertyData + ".intValue",
                    Constants.DatabaseSchema.Tables.PropertyData + ".textValue",
                    Constants.DatabaseSchema.Tables.PropertyData + ".varcharValue",
                    "cmsPropertyType.id",
                    "cmsPropertyType.Alias",
                    "cmsPropertyType.Description",
                    "cmsPropertyType.Name",
                    "cmsPropertyType.mandatory",
                    "cmsPropertyType.validationRegExp",
                    "cmsPropertyType.sortOrder AS PropertyTypeSortOrder",
                    "cmsPropertyType.propertyTypeGroupId",
                    "cmsPropertyType.dataTypeId",
                    "cmsDataType.propertyEditorAlias",
                    "cmsDataType.dbType")
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
            IScopeProvider provider = ScopeProvider;
            Sql<ISqlContext> sql = ScopeAccessor.AmbientScope.SqlContext.Sql();
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
