using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests
{
	[TestFixture]
	public class DynamicNodeTests : BaseWebTest
	{
		private DynamicNode GetDynamicNode(int id)
		{
			var template = Template.MakeNew("test", new User(0));
			var ctx = GetUmbracoContext("/test", template);
			var contentStore = new XmlPublishedContentStore();
			var doc = contentStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			var dynamicNode = new DynamicNode(doc);
			Assert.IsNotNull(dynamicNode);
			return dynamicNode;
		}

		[Test]
		public void Get_Children()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var children = asDynamic.Children;
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(children));

			var childrenAsList = asDynamic.ChildrenAsList; //test ChildrenAsList too
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(childrenAsList));

			var castChildren = (IEnumerable<DynamicNode>) children;
			Assert.AreEqual(2, castChildren.Count());

			var castChildrenAsList = (IEnumerable<DynamicNode>)childrenAsList;
			Assert.AreEqual(2, castChildrenAsList.Count());
		}

		[Test]
		public void Ancestor_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var aos = asDynamic.AncestorOrSelf();

			Assert.IsNotNull(aos);

			Assert.AreEqual(1046, aos.Id);
		}

		[Test]
		public void Ancestors_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			var aos = asDynamic.AncestorsOrSelf();

			Assert.IsNotNull(aos);

			var list = (IEnumerable<DynamicNode>) aos;
			Assert.AreEqual(3, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1174, 1173, 1046 }));
		}
	}
}