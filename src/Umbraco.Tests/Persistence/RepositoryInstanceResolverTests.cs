using System;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Persistence
{
	[TestFixture]
	public class RepositoryInstanceResolverTests
	{
		[SetUp]
		public void Setup()
		{
			RepositoryInstanceResolver.Current = new RepositoryInstanceResolver(
				new RepositoryInstanceFactory());
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryInstanceResolver.Reset();
		}

		[TestCase(typeof(IUserTypeRepository))]
		[TestCase(typeof(IUserRepository))]
		[TestCase(typeof(IContentRepository))]
		[TestCase(typeof(IMediaRepository))]
		[TestCase(typeof(IMediaTypeRepository))]
		[TestCase(typeof(IContentTypeRepository))]
		[TestCase(typeof(IDataTypeDefinitionRepository))]
		[TestCase(typeof(IDictionaryRepository))]
		[TestCase(typeof(ILanguageRepository))]
		[TestCase(typeof(IMacroRepository))]		
		[TestCase(typeof(IRelationRepository))]
		[TestCase(typeof(IRelationTypeRepository))]
		[TestCase(typeof(IScriptRepository))]
		[TestCase(typeof(IStylesheetRepository))]
		[TestCase(typeof(ITemplateRepository))]
		public void ResolveRepository(Type repoType)
		{
			var method = typeof (RepositoryInstanceResolver).GetMethod("ResolveByType", BindingFlags.NonPublic | BindingFlags.Instance);
			var gMethod = method.MakeGenericMethod(repoType);
			var repo = gMethod.Invoke(RepositoryInstanceResolver.Current, new object[] {new PetaPocoUnitOfWork()});			
			Assert.IsNotNull(repo);
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom(repoType, repo.GetType()));
		}
		
	}
}