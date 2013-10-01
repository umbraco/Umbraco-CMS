using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MemberRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private MemberRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out MemberTypeRepository memberTypeRepository)
        {
            memberTypeRepository = new MemberTypeRepository(unitOfWork, NullCacheProvider.Current);
            var repository = new MemberRepository(unitOfWork, NullCacheProvider.Current, memberTypeRepository);
            return repository;
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IMemberRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Get_Member_By_Id()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                var member = repository.Get(1341);

                Assert.That(member, Is.Not.Null);
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Get_Specific_Members()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                var members = repository.GetAll(1341, 1383);

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Count(), Is.EqualTo(2));
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Get_All_Members()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                var members = repository.GetAll();

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Count(), Is.EqualTo(6));
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Perform_GetByQuery_With_Key()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                // Act
                var query = Query<IMember>.Builder.Where(x => x.Key == new Guid("A6B9CA6B-0615-42CA-B5F5-338417EC76BE"));
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(1341));
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Perform_GetByQuery_With_Property_Value()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                // Act
                var query = Query<IMember>.Builder.Where(x => ((Member) x).ShortStringPropertyValue.EndsWith("piquet_h"));
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Any(x => x == null), Is.False);
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(1341));
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberRepository_Can_Perform_GetByQuery_With_Property_Alias_And_Value()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {

                // Act
                var query = Query<IMember>.Builder.Where(x => ((Member) x).LongStringPropertyValue.Contains("1095") && ((Member) x).PropertyTypeAlias == "headshot");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Any(x => x == null), Is.False);
                Assert.That(result.Count(), Is.EqualTo(5));
            }
        }

        [Test]
        public void MemberRepository_Can_Persist_Member()
        {
            IMember sut;
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var member = MockedMember.CreateSimpleContent(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty", -1);
                repository.AddOrUpdate(member);
                unitOfWork.Commit();

                sut = repository.Get(member.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.ContentType.PropertyGroups.Count(), Is.EqualTo(1));
                Assert.That(sut.ContentType.PropertyTypes.Count(), Is.EqualTo(12));

                Assert.That(sut.Properties.Count(), Is.EqualTo(12));
                Assert.That(sut.Properties.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
            }
        }

        [Test]
        public void Can_Create_Correct_Subquery()
        {
            var query = Query<IMember>.Builder.Where(x =>
                        ((Member) x).LongStringPropertyValue.Contains("1095") &&
                        ((Member) x).PropertyTypeAlias == "headshot");

            var sqlSubquery = GetSubquery();
            var translator = new SqlTranslator<IMember>(sqlSubquery, query);
            var subquery = translator.Translate();
            var sql = GetBaseQuery(false)
                .Append(new Sql("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments))
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            Console.WriteLine(sql.SQL);
            Assert.That(sql.SQL, Is.Not.Empty);
        }

        private Sql GetBaseQuery(bool isCount)
        {
            if (isCount)
            {
                var sqlCount = new Sql()
                    .Select("COUNT(*)")
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sqlCount;
            }

            var sql = new Sql();
            sql.Select("umbracoNode.*", "cmsContent.contentType", "cmsContentType.alias AS ContentTypeAlias", "cmsContentVersion.VersionId",
                "cmsContentVersion.VersionDate", "cmsContentVersion.LanguageLocale", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password", "cmsPropertyData.id AS PropertyDataId", "cmsPropertyData.propertytypeid",
                "cmsPropertyData.dataDate", "cmsPropertyData.dataInt", "cmsPropertyData.dataNtext", "cmsPropertyData.dataNvarchar",
                "cmsPropertyType.id", "cmsPropertyType.Alias", "cmsPropertyType.Description",
                "cmsPropertyType.Name", "cmsPropertyType.mandatory", "cmsPropertyType.validationRegExp",
                "cmsPropertyType.helpText", "cmsPropertyType.sortOrder AS PropertyTypeSortOrder", "cmsPropertyType.propertyTypeGroupId",
                "cmsPropertyType.dataTypeId", "cmsDataType.propertyEditorAlias", "cmsDataType.dbType")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        private Sql GetSubquery()
        {
            var sql = new Sql();
            sql.Select("umbracoNode.id")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        private Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Member); }
        }
    }
}