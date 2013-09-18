using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture, NUnit.Framework.Ignore]
    public class MemberTypeRepositoryTest : MemberRepositoryBaseTest
    {
        #region Overrides of MemberRepositoryBaseTest

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            SqlSyntaxContext.SqlSyntaxProvider = SqlServerSyntax.Provider;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public override string ConnectionString
        {
            get { return @"server=.\SQLEXPRESS;database=Kloud-Website-Production;user id=umbraco;password=umbraco"; }
        }

        public override string ProviderName
        {
            get { return "System.Data.SqlClient"; }
        }

        #endregion

        private MemberTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            return new MemberTypeRepository(unitOfWork, NullCacheProvider.Current);            
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IMemberTypeRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void MemberTypeRepository_Can_Get_MemberType_By_Id()
        {
            var unitOfWork = UnitOfWorkProvider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var memberType = repository.Get(1340);

                Assert.That(memberType, Is.Not.Null);
                Assert.That(memberType.PropertyTypes.Count(), Is.EqualTo(13));
                Assert.That(memberType.PropertyGroups.Any(), Is.False);

                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();
                Assert.That(memberType.PropertyTypes.Any(x => x.HasIdentity == false), Is.False);
            }
        }
    }
}