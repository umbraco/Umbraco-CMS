using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.DynamicDocument
{
	[TestFixture]
	public abstract class DynamicDocumentTestsBase<TDocument, TDocumentList> : BaseWebTest
	{
		/// <summary>
		/// Returns the dynamic node/document to run tests against
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected abstract dynamic GetDynamicNode(int id);

		[Test]
		public void Children_Order_By_Update_Date()
		{
			var asDynamic = GetDynamicNode(1173);

			var ordered = asDynamic.Children.OrderBy("UpdateDate");
			var casted = (IEnumerable<TDocument>)ordered;

			var correctOrder = new[] { 1178, 1177, 1174, 1176 };
			for (var i = 0; i < correctOrder.Length ;i++)
			{
				Assert.AreEqual(correctOrder[i], ((dynamic)casted.ElementAt(i)).Id);
			}

		}

		[Test]
		public void Take()
		{
			var asDynamic = GetDynamicNode(1173);

			var ordered = asDynamic.Children.OrderBy("UpdateDate");
			var take = ordered.Take(2);
			var casted = (IEnumerable<TDocument>)take;

			Assert.AreEqual(2, casted.Count());

		}

		[Test]
		public void Ancestors_Where_Visible()
		{
			var asDynamic = GetDynamicNode(1174);

			var whereVisible = asDynamic.Ancestors().Where("Visible");
			var casted = (IEnumerable<TDocument>)whereVisible;

			Assert.AreEqual(1, casted.Count());
			
		}

		[Test]
		public void Visible()
		{
			var asDynamicHidden = GetDynamicNode(1046);
			var asDynamicVisible = GetDynamicNode(1173);

			Assert.IsFalse(asDynamicHidden.Visible);
			Assert.IsTrue(asDynamicVisible.Visible);
		}

		[Test]
		public void Ensure_TinyMCE_Converted_Type_User_Property()
		{
			var asDynamic = GetDynamicNode(1173);

			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IHtmlString>(asDynamic.Content.GetType()));
			Assert.AreEqual("<div>This is some content</div>", asDynamic.Content.ToString());
		}

		[Test]
		public void Get_Children_With_Pluralized_Alias()
		{
			var asDynamic = GetDynamicNode(1173);

			Action<object> doAssert = d =>
				{
					Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(d));
					var casted = (IEnumerable<TDocument>)d;
					Assert.AreEqual(2, casted.Count());
				};

			doAssert(asDynamic.Homes); //pluralized alias
			doAssert(asDynamic.homes); //pluralized alias
			doAssert(asDynamic.CustomDocuments); //pluralized alias			
			doAssert(asDynamic.customDocuments); //pluralized alias
		}

		[Test]
		public void GetPropertyValue_Non_Reflected()
		{
			var asDynamic = GetDynamicNode(1174);

			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.GetPropertyValue("creatorName"));
			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.GetPropertyValue("CreatorName"));
		}

		[Test]
		public void GetPropertyValue_Reflected()
		{
			var asDynamic = GetDynamicNode(1174);

			Assert.AreEqual("admin", asDynamic.GetPropertyValue("@creatorName"));
			Assert.AreEqual("admin", asDynamic.GetPropertyValue("@CreatorName"));
		}

		[Test]
		public void Get_User_Property_With_Same_Name_As_Member_Property()
		{
			var asDynamic = GetDynamicNode(1174);

			Assert.AreEqual("Custom data with same property name as the member name", asDynamic.creatorName);

			//because CreatorName is defined on DynamicNode, it will not return the user defined property
			Assert.AreEqual("admin", asDynamic.CreatorName);
		}

		[Test]
		public void Get_Member_Property()
		{
			var asDynamic = GetDynamicNode(1173);
			
			Assert.AreEqual((int) 2, (int) asDynamic.Level);
			Assert.AreEqual((int) 2, (int) asDynamic.level);

			Assert.AreEqual((int) 1046, (int) asDynamic.ParentId);
			Assert.AreEqual((int) 1046, (int) asDynamic.parentId);
		}

		[Test]
		public void Get_Children()
		{
			var asDynamic = GetDynamicNode(1173);

			var children = asDynamic.Children;
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(children));

			var childrenAsList = asDynamic.ChildrenAsList; //test ChildrenAsList too
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom<IEnumerable>(childrenAsList));

			var castChildren = (IEnumerable<TDocument>)children;
			Assert.AreEqual(4, castChildren.Count());

			var castChildrenAsList = (IEnumerable<TDocument>)childrenAsList;
			Assert.AreEqual(4, castChildrenAsList.Count());
		}

		[Test]
		public void Ancestor_Or_Self()
		{
			var asDynamic = GetDynamicNode(1173);

			var result = asDynamic.AncestorOrSelf();

			Assert.IsNotNull(result);

			Assert.AreEqual((int) 1046, (int) result.Id);
		}

		[Test]
		public void Ancestors_Or_Self()
		{
			var asDynamic = GetDynamicNode(1174);

			var result = asDynamic.AncestorsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<TDocument>)result;
			Assert.AreEqual(3, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1174, 1173, 1046 }));
		}

		[Test]
		public void Ancestors()
		{
			var asDynamic = GetDynamicNode(1174);

			var result = asDynamic.Ancestors();

			Assert.IsNotNull(result);

			var list = (IEnumerable<TDocument>)result;
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1173, 1046 }));
		}

		[Test]
		public void Descendants_Or_Self()
		{
			var asDynamic = GetDynamicNode(1046);

			var result = asDynamic.DescendantsOrSelf();

			Assert.IsNotNull(result);

			var list = (IEnumerable<TDocument>)result;
			Assert.AreEqual(7, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1046, 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Descendants()
		{
			var asDynamic = GetDynamicNode(1046);

			var result = asDynamic.Descendants();

			Assert.IsNotNull(result);

			var list = (IEnumerable<TDocument>)result;
			Assert.AreEqual(6, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1173, 1174, 1176, 1175 }));
		}

		[Test]
		public void Up()
		{
			var asDynamic = GetDynamicNode(1173);

			var result = asDynamic.Up();

			Assert.IsNotNull(result);

			Assert.AreEqual((int) 1046, (int) result.Id);
		}

		[Test]
		public void Down()
		{
			var asDynamic = GetDynamicNode(1173);

			var result = asDynamic.Down();

			Assert.IsNotNull(result);

			Assert.AreEqual((int) 1174, (int) result.Id);
		}

		[Test]
		public void Next()
		{
			var asDynamic = GetDynamicNode(1173);

			var result = asDynamic.Next();

			Assert.IsNotNull(result);

			Assert.AreEqual((int) 1175, (int) result.Id);
		}

		[Test]
		public void Next_Without_Sibling()
		{
			var asDynamic = GetDynamicNode(1178);

			Assert.IsNull(asDynamic.Next());
		}

		[Test]
		public void Previous_Without_Sibling()
		{
			var asDynamic = GetDynamicNode(1173);

			Assert.IsNull(asDynamic.Previous());
		}

		[Test]
		public void Previous()
		{
			var asDynamic = GetDynamicNode(1176);

			var result = asDynamic.Previous();

			Assert.IsNotNull(result);

			Assert.AreEqual((int) 1174, (int) result.Id);
		}

	}
}