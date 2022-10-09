// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
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

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

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
        var tagRepo = GetRequiredService<ITagRepository>();
        var relationTypeRepository = GetRequiredService<IRelationTypeRepository>();
        var relationRepository = GetRequiredService<IRelationRepository>();
        var propertyEditors =
            new PropertyEditorCollection(new DataEditorCollection(() => Enumerable.Empty<IDataEditor>()));
        var dataValueReferences =
            new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var member = CreateTestMember();

            member = repository.Get(member.Id);

            Assert.That(member, Is.Not.Null);
            Assert.That(member.HasIdentity, Is.True);
        }
    }

    [Test]
    public void GetMembers()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var type = CreateTestMemberType();
            var m1 = CreateTestMember(type, "Test 1", "test1@test.com", "pass1", "test1");
            var m2 = CreateTestMember(type, "Test 2", "test2@test.com", "pass2", "test2");

            var members = repository.GetMany(m1.Id, m2.Id).ToArray();

            Assert.That(members, Is.Not.Null);
            Assert.That(members.Count(), Is.EqualTo(2));
            Assert.That(members.Any(x => x == null), Is.False);
            Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
        }
    }

    [Test]
    public void GetAllMembers()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var type = CreateTestMemberType();
            for (var i = 0; i < 5; i++)
            {
                CreateTestMember(type, "Test " + i, "test" + i + "@test.com", "pass" + i, "test" + i);
            }

            var members = repository.GetMany().ToArray();

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var key = Guid.NewGuid();
            var member = CreateTestMember(key: key);

            // Act
            var query = provider.CreateQuery<IMember>().Where(x => x.Key == key);
            var result = repository.Get(query).ToArray();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(member.Id));
        }
    }

    [Test]
    public void SaveMember()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var member = CreateTestMember();

            var sut = repository.Get(member.Id);

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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeRepository.Save(memberType);

            var member =
                MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
            repository.Save(member);

            var sut = repository.Get(member.Id);

            Assert.That(memberType.CompositionPropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(memberType.CompositionPropertyTypes.Count(), Is.EqualTo(3 + ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Count));
            Assert.That(sut.Properties.Count(), Is.EqualTo(3 + ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Count));
            var grp = memberType.CompositionPropertyGroups.FirstOrDefault(x =>
                x.Name == Constants.Conventions.Member.StandardPropertiesGroupName);
            Assert.IsNotNull(grp);
            var aliases = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper).Select(x => x.Key)
                .ToArray();
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeRepository.Save(memberType);

            var member =
                MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
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
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeRepository.Save(memberType);

            var member =
                MemberBuilder.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
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
        var provider = ScopeProvider;

        using (provider.CreateScope())
        {
            var query = provider.CreateQuery<IMember>().Where(x =>
                ((Member)x).LongStringPropertyValue.Contains("1095") &&
                ((Member)x).PropertyTypeAlias == "headshot");

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
    }

    private IMember CreateTestMember(IMemberType memberType = null, string name = null, string email = null, string password = null, string username = null, Guid? key = null)
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            if (memberType == null)
            {
                memberType = MemberTypeBuilder.CreateSimpleMemberType();
                MemberTypeRepository.Save(memberType);
            }

            var member = MemberBuilder.CreateSimpleMember(memberType, name ?? "Johnny Hefty", email ?? "johnny@example.com", password ?? "123", username ?? "hefty", key);
            repository.Save(member);
            scope.Complete();

            return member;
        }
    }

    private IMemberType CreateTestMemberType(string alias = null)
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = MemberTypeBuilder.CreateSimpleMemberType(alias);
            MemberTypeRepository.Save(memberType);
            scope.Complete();
            return memberType;
        }
    }

    private Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        var provider = ScopeProvider;
        if (isCount)
        {
            var sqlCount = ScopeAccessor.AmbientScope.SqlContext.Sql()
                .SelectCount()
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>()
                .On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sqlCount;
        }

        var sql = ScopeAccessor.AmbientScope.SqlContext.Sql();
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
            .InnerJoin<ContentTypeDto>()
            .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
            .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .LeftJoin<PropertyTypeDto>()
            .On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
            .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
            .LeftJoin<PropertyDataDto>()
            .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
            .Append("AND " + Constants.DatabaseSchema.Tables.PropertyData +
                    $".versionId = {Constants.DatabaseSchema.Tables.ContentVersion}.id")
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        return sql;
    }

    private Sql<ISqlContext> GetSubquery()
    {
        var provider = ScopeProvider;
        var sql = ScopeAccessor.AmbientScope.SqlContext.Sql();
        sql.Select("umbracoNode.id")
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
            .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .LeftJoin<PropertyTypeDto>()
            .On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
            .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
            .LeftJoin<PropertyDataDto>()
            .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
            .Append("AND " + Constants.DatabaseSchema.Tables.PropertyData +
                    $".versionId = {Constants.DatabaseSchema.Tables.ContentVersion}.id")
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        return sql;
    }

    private Guid NodeObjectTypeId => Constants.ObjectTypes.Member;
}
