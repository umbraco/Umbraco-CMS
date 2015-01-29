using System;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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

        private MemberRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out MemberTypeRepository memberTypeRepository, out MemberGroupRepository memberGroupRepository)
        {
            memberTypeRepository = new MemberTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            memberGroupRepository = new MemberGroupRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, CacheHelper.CreateDisabledCacheHelper());
            var tagRepo = new TagRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);
            var repository = new MemberRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax, memberTypeRepository, memberGroupRepository, tagRepo);
            return repository;
        }

        [Test]
        public void Rebuild_All_Xml_Structures()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var memberType1 = CreateTestMemberType();
                
                for (var i = 0; i < 100; i++)
                {
                    var member = MockedMember.CreateSimpleMember(memberType1, "blah" + i, "blah" + i + "@example.com", "blah", "blah" + i);
                    repository.AddOrUpdate(member);
                }
                unitOfWork.Commit();

                //delete all xml
                unitOfWork.Database.Execute("DELETE FROM cmsContentXml");
                Assert.AreEqual(0, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));

                repository.RebuildXmlStructures(media => new XElement("test"), 10);

                Assert.AreEqual(100, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));
            }
        }

        [Test]
        public void Rebuild_All_Xml_Structures_For_Content_Type()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {

                var memberType1 = CreateTestMemberType("mt1");
                var memberType2 = CreateTestMemberType("mt2");
                var memberType3 = CreateTestMemberType("mt3");

                for (var i = 0; i < 30; i++)
                {
                    var member = MockedMember.CreateSimpleMember(memberType1, "b1lah" + i, "b1lah" + i + "@example.com", "b1lah", "b1lah" + i);
                    repository.AddOrUpdate(member);
                }
                for (var i = 0; i < 30; i++)
                {
                    var member = MockedMember.CreateSimpleMember(memberType2, "b2lah" + i, "b2lah" + i + "@example.com", "b2lah", "b2lah" + i);
                    repository.AddOrUpdate(member);
                }
                for (var i = 0; i < 30; i++)
                {
                    var member = MockedMember.CreateSimpleMember(memberType3, "b3lah" + i, "b3lah" + i + "@example.com", "b3lah", "b3lah" + i);
                    repository.AddOrUpdate(member);
                }
                unitOfWork.Commit();

                //delete all xml                 
                unitOfWork.Database.Execute("DELETE FROM cmsContentXml");
                Assert.AreEqual(0, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));

                repository.RebuildXmlStructures(media => new XElement("test"), 10, contentTypeIds: new[] { memberType1.Id, memberType2.Id });

                Assert.AreEqual(60, unitOfWork.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContentXml"));
            }
        }



        [Test]
        public void MemberRepository_Can_Get_Member_By_Id()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var member = CreateTestMember();

                member = repository.Get(member.Id);

                Assert.That(member, Is.Not.Null);
                Assert.That(member.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Get_Members_By_Ids()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var type = CreateTestMemberType();
                var m1 = CreateTestMember(type, "Test 1", "test1@test.com", "pass1", "test1");
                var m2 = CreateTestMember(type, "Test 2", "test2@test.com", "pass2", "test2");

                var members = repository.GetAll(m1.Id, m2.Id);

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Count(), Is.EqualTo(2));
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
            }
        }

        [Test]
        public void MemberRepository_Can_Get_All_Members()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var type = CreateTestMemberType();
                for (var i = 0; i < 5; i++)
                {
                    CreateTestMember(type, "Test " + i, "test" + i + "@test.com", "pass" + i, "test" + i);
                }

                var members = repository.GetAll();

                Assert.That(members, Is.Not.Null);
                Assert.That(members.Any(x => x == null), Is.False);
                Assert.That(members.Count(), Is.EqualTo(5));
                Assert.That(members.Any(x => x.HasIdentity == false), Is.False);
            }
        }

        [Test]
        public void MemberRepository_Can_Perform_GetByQuery_With_Key()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var key = Guid.NewGuid();
                var member = CreateTestMember(key: key);

                // Act
                var query = Query<IMember>.Builder.Where(x => x.Key == key);
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Id, Is.EqualTo(member.Id));
            }
        }

        [Test]
        public void Can_Persist_Member()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var member = CreateTestMember();

                var sut = repository.Get(member.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.HasIdentity, Is.True);      
                Assert.That(sut.Properties.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.Name, Is.EqualTo("Johnny Hefty"));
                Assert.That(sut.Email, Is.EqualTo("johnny@example.com"));
                Assert.That(sut.RawPasswordValue, Is.EqualTo("123"));
                Assert.That(sut.Username, Is.EqualTo("hefty"));      
            }
        }

        [Test]
        public void New_Member_Has_Built_In_Properties_By_Default()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.AddOrUpdate(member);
                unitOfWork.Commit();

                var sut = repository.Get(member.Id);

                Assert.That(sut.ContentType.PropertyGroups.Count(), Is.EqualTo(2));
                Assert.That(sut.ContentType.PropertyTypes.Count(), Is.EqualTo(3 + Constants.Conventions.Member.GetStandardPropertyTypeStubs().Count));
                Assert.That(sut.Properties.Count(), Is.EqualTo(3 + Constants.Conventions.Member.GetStandardPropertyTypeStubs().Count));
                Assert.That(sut.Properties.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                var grp = sut.PropertyGroups.FirstOrDefault(x => x.Name == Constants.Conventions.Member.StandardPropertiesGroupName);
                Assert.IsNotNull(grp);
                var aliases = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
                foreach (var p in sut.PropertyTypes.Where(x => aliases.Contains(x.Alias)))
                {
                    Assert.AreEqual(grp.Id, p.PropertyGroupId.Value);
                }
            }
        }

        [Test]
        public void MemberRepository_Does_Not_Replace_Password_When_Null()
        {
            IMember sut;
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.AddOrUpdate(member);
                unitOfWork.Commit();

                sut = repository.Get(member.Id);
                //when the password is null it will not overwrite what is already there.
                sut.RawPasswordValue = null;
                repository.AddOrUpdate(sut);
                unitOfWork.Commit();
                sut = repository.Get(member.Id);

                Assert.That(sut.RawPasswordValue, Is.EqualTo("123"));
            }
        }

        [Test]
        public void MemberRepository_Can_Update_Email_And_Login_When_Changed()
        {
            IMember sut;
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberTypeRepository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var member = MockedMember.CreateSimpleMember(memberType, "Johnny Hefty", "johnny@example.com", "123", "hefty");
                repository.AddOrUpdate(member);
                unitOfWork.Commit();

                sut = repository.Get(member.Id);
                sut.Username = "This is new";
                sut.Email = "thisisnew@hello.com";
                repository.AddOrUpdate(sut);
                unitOfWork.Commit();
                sut = repository.Get(member.Id);

                Assert.That(sut.Email, Is.EqualTo("thisisnew@hello.com"));
                Assert.That(sut.Username, Is.EqualTo("This is new"));
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

        private IMember CreateTestMember(IMemberType memberType = null, string name = null, string email = null, string password = null, string username = null, Guid? key = null)
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                if (memberType == null)
                {
                    memberType = MockedContentTypes.CreateSimpleMemberType();
                    memberTypeRepository.AddOrUpdate(memberType);
                    unitOfWork.Commit();    
                }

                var member = MockedMember.CreateSimpleMember(memberType, name ?? "Johnny Hefty", email ?? "johnny@example.com", password ?? "123", username ?? "hefty", key);
                repository.AddOrUpdate(member);
                unitOfWork.Commit();

                return member;
            }
        }

        private IMemberType CreateTestMemberType(string alias = null)
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            MemberTypeRepository memberTypeRepository;
            MemberGroupRepository memberGroupRepository;
            using (var repository = CreateRepository(unitOfWork, out memberTypeRepository, out memberGroupRepository))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType(alias);
                memberTypeRepository.AddOrUpdate(memberType);
                unitOfWork.Commit();
                return memberType;
            }
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
                "cmsContentVersion.VersionDate", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password", "cmsPropertyData.id AS PropertyDataId", "cmsPropertyData.propertytypeid",
                "cmsPropertyData.dataDate", "cmsPropertyData.dataInt", "cmsPropertyData.dataNtext", "cmsPropertyData.dataNvarchar",
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