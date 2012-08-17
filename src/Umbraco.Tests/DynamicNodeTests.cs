using System;
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

			var castChildren = (IEnumerable<DynamicNode>)children;
			Assert.AreEqual(2, castChildren.Count());

			var castChildrenAsList = (IEnumerable<DynamicNode>)childrenAsList;
			Assert.AreEqual(2, castChildrenAsList.Count());
		}

		[Test]
		public void Ancestor_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.AncestorOrSelf();

			Assert.IsNotNull(result);

			Assert.AreEqual(1046, result.Id);
		}

		[Test]
		public void Ancestors_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.AncestorsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicNode>)result;
			Assert.AreEqual(3, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1174, 1173, 1046 }));
		}

		[Test]
		public void Ancestors()
		{
			var dynamicNode = GetDynamicNode(1174);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Ancestors();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicNode>)result;
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1173, 1046 }));
		}

		[Test]
		public void Descendants_Or_Self()
		{
			var dynamicNode = GetDynamicNode(1046);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.DescendantsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicNode>)result;
			Assert.AreEqual(5, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1046, 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Descendants()
		{
			var dynamicNode = GetDynamicNode(1046);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Descendants();

			Assert.IsNotNull(result);

			var list = (IEnumerable<DynamicNode>)result;
			Assert.AreEqual(4, list.Count());
			Assert.IsTrue(list.Select(x => x.Id).ContainsAll(new[] { 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Up()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Up();

			Assert.IsNotNull(result);

			Assert.AreEqual(1046, result.Id);
		}

		[Test]
		public void Down()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Down();

			Assert.IsNotNull(result);

			Assert.AreEqual(1174, result.Id);
		}

		[Test]
		public void Next()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Next();

			Assert.IsNotNull(result);

			Assert.AreEqual(1175, result.Id);
		}

		[Test]
		public void Next_Without_Sibling()
		{
			var dynamicNode = GetDynamicNode(1176);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.IsNull(asDynamic.Next());
		}

		[Test]
		public void Previous_Without_Sibling()
		{
			var dynamicNode = GetDynamicNode(1173);
			var asDynamic = dynamicNode.AsDynamic();

			Assert.IsNull(asDynamic.Previous());
		}

		[Test]
		public void Previous()
		{
			var dynamicNode = GetDynamicNode(1176);
			var asDynamic = dynamicNode.AsDynamic();

			var result = asDynamic.Previous();

			Assert.IsNotNull(result);

			Assert.AreEqual(1174, result.Id);
		}
	}
}