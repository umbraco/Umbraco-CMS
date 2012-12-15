using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
	[TestFixture, RequiresSTA]
	public class ContentTypeServiceTests : BaseServiceTest
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

		[Test]
		public void Can_Bulk_Save_New_Hierarchy_Content()
		{
			// Arrange
			var contentTypeService = ServiceContext.ContentTypeService;
			var hierarchy = CreateContentTypeHierarchy();

			// Act
			contentTypeService.Save(hierarchy, 0);

			Assert.That(hierarchy.Any(), Is.True);
			Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);
			//all parent id's should be ok, they are lazy and if they equal zero an exception will be thrown
			Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));
			for (var i = 0; i < hierarchy.Count(); i++)
			{
				if (i == 0) continue;
				Assert.AreEqual(hierarchy.ElementAt(i).ParentId, hierarchy.ElementAt(i - 1).Id);
			}
		}

		private IEnumerable<IContentType> CreateContentTypeHierarchy()
		{
			//create the master type
			var masterContentType = MockedContentTypes.CreateSimpleContentType("masterContentType", "MasterContentType");
			masterContentType.Key = new Guid("C00CA18E-5A9D-483B-A371-EECE0D89B4AE");
			ServiceContext.ContentTypeService.Save(masterContentType);

			//add the one we just created
			var list = new List<IContentType> {masterContentType};
			
			for (var i = 0; i < 10; i++)
			{
				var contentType = MockedContentTypes.CreateSimpleContentType("childType" + i, "ChildType" + i, 
					//make the last entry in the list, this one's parent
					list.Last());

				list.Add(contentType);				
			}

			return list;
		}
	}
}