using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PublishedContent
{
	[TestFixture]
    public abstract class DynamicDocumentTestsBase<TDocument, TDocumentList> : PublishedContentTestBase
	{
        private IUmbracoSettingsSection _umbracoSettings;

        public override void Initialize()
        {
            // required so we can access property.Value
            //PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver();

            base.Initialize();

            //generate new mock settings and assign so we can configure in individual tests
            _umbracoSettings = SettingsForTests.GenerateMockSettings();
            SettingsForTests.ConfigureSettings(_umbracoSettings);

            var scriptingMock = Mock.Get(_umbracoSettings.Scripting);
            scriptingMock.Setup(x => x.DataTypeModelStaticMappings).Returns(new List<IRazorStaticMapping>());

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("umbracoNaviHide", 0, "?"), 
                    new PublishedPropertyType("selectedNodes", 0, "?"), 
                    new PublishedPropertyType("umbracoUrlAlias", 0, "?"), 
                    new PublishedPropertyType("content", 0, Constants.PropertyEditors.TinyMCEAlias), 
                    new PublishedPropertyType("testRecursive", 0, "?"), 
                    new PublishedPropertyType("siteTitle", 0, "?"), 
                    new PublishedPropertyType("creatorName", 0, "?"), 
                    new PublishedPropertyType("blah", 0, "?"), // ugly error when that one is missing...
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;

        }
        
        protected override string GetXmlContent(int templateId)
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
	<Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
		<content><![CDATA[]]></content>
		<umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
		<umbracoNaviHide>1</umbracoNaviHide>
        <siteTitle><![CDATA[This is my site]]></siteTitle>
		<Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>			
			<Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
				<content><![CDATA[]]></content>
				<umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
				<creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
			</Home>			
			<CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
			<CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />
            <Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[some content]]></content>
				<blah><![CDATA[some content]]></blah>
                <umbracoNaviHide>1</umbracoNaviHide>
			</Home>
		</Home>
		<Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
		</Home>
		<CustomDocument id=""4444"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,4444"" isDoc="""">
			<selectedNodes><![CDATA[1172,1176,1173]]></selectedNodes>
			<CustomDocument id=""5555"" parentID=""1046"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""0"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,4444,5555"" isDoc="""">
			</CustomDocument>
		</CustomDocument>
	</Home>
	<CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
		}

		/// <summary>
		/// Returns the dynamic node/document to run tests against
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected abstract dynamic GetDynamicNode(int id);

        [Test]
        public void Recursive_Property()
        {
            var doc = GetDynamicNode(1174);
            var prop = doc.GetProperty("siteTitle", true);
            Assert.IsNotNull(prop);
            Assert.AreEqual("This is my site", prop.Value);
            prop = doc.GetProperty("_siteTitle"); //test with underscore prefix
            Assert.IsNotNull(prop);
            Assert.AreEqual("This is my site", prop.Value);
            Assert.AreEqual("This is my site", doc._siteTitle);
        }

        /// <summary>
        /// Tests the internal instance level caching of returning properties
        /// </summary>
        /// <remarks>
        /// http://issues.umbraco.org/issue/U4-1824
        /// http://issues.umbraco.org/issue/U4-1825
        /// </remarks>
        [Test]
        public void Can_Return_Property_And_Value()
        {
            var doc = GetDynamicNode(1173);

            Assert.IsTrue(doc.HasProperty(Constants.Conventions.Content.UrlAlias));
            var prop = doc.GetProperty(Constants.Conventions.Content.UrlAlias);
            Assert.IsNotNull(prop);
            Assert.AreEqual("page2/alias, 2ndpagealias", prop.Value);
            Assert.AreEqual("page2/alias, 2ndpagealias", doc.umbracoUrlAlias);
        }

        /// <summary>
        /// Tests the IsLast method with the result set from a Where statement
        /// </summary>
        [Test]
        public void Is_Last_From_Where_Filter()
        {
            var doc = GetDynamicNode(1173);

            foreach (var d in doc.Children.Where("Visible"))
            {
                if (d.Id != 1178)
                {
                    Assert.IsFalse(d.IsLast());
                }
                else
                {
                    Assert.IsTrue(d.IsLast());
                }
            }

        }

		[Test]
		public void Single()
		{
			var doc = GetDynamicNode(4444);

			var result = doc.Children().Single();				

			Assert.IsNotNull(result);
			Assert.AreEqual(5555, result.Id);
		}

		[Test]
		public void Single_With_Query()
		{
			var doc = GetDynamicNode(1046);

			var result = doc.Children().Single("id==1175");

			Assert.IsNotNull(result);
			Assert.AreEqual(1175, result.Id);
		}

		[Test]
		public void First()
		{
			var doc = GetDynamicNode(1173);

			var result = doc.Children().First();

			Assert.IsNotNull(result);
			Assert.AreEqual(1174, result.Id);
		}

		[Test]
		public void First_With_Query()
		{
			var doc = GetDynamicNode(1173);

			var result = doc.Children().First("blah==\"some content\"");

			Assert.IsNotNull(result);
			Assert.AreEqual(1176, result.Id);
		}

		[Test]
		public void Where_User_Property_Value()
		{
			var doc = GetDynamicNode(1173);

			var result = (IEnumerable<dynamic>)doc.Children().Where("blah==\"some content\"");

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(1176, result.Single().Id);		
		}

		[Test]
		public void String_ContainsValue_Extension_Method()
		{
			var doc = GetDynamicNode(1046);

			var paramVals = new Dictionary<string, object> { { "searchId", 1173 } }; //this is an integer value
			var result = doc.Children()
				.Where("selectedNodes.ContainsValue(searchId)", paramVals) //call an extension method
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.AreEqual(4444, result.Id);

			//don't find!
			paramVals = new Dictionary<string, object> { { "searchId", 1111777 } };
			result = doc.Children()
				.Where("selectedNodes.ContainsValue(searchId)", paramVals)
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.IsTrue(result.GetType() == typeof(DynamicNull) || result.GetType() == typeof(umbraco.MacroEngines.DynamicNull));
			//Assert.AreEqual(typeof(DynamicNull), result.GetType());
		}

		[Test]
		public void String_Contains_Method()
		{
			var doc = GetDynamicNode(1046);

			var paramVals = new Dictionary<string, object> { { "searchId", "1173" } };
			var result = doc.Children()
				.Where("selectedNodes.Contains(searchId)", paramVals)
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.AreEqual(4444, result.Id);

			//don't find!
			paramVals = new Dictionary<string, object> { { "searchId", "1aaa173" } };
			result = doc.Children()
				.Where("selectedNodes.Contains(searchId)", paramVals)
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.IsTrue(result.GetType() == typeof (DynamicNull) || result.GetType() == typeof (umbraco.MacroEngines.DynamicNull));
			//Assert.AreEqual(typeof (DynamicNull), result.GetType());
		}

		[Test]
		public void String_Split_Method()
		{
			var doc = GetDynamicNode(1046);

			var paramVals = new Dictionary<string, object>
				{
					{ "splitTerm", new char[] { ',' } },
					{ "splitOptions", StringSplitOptions.RemoveEmptyEntries }
				};
			var result = doc.Children()
				.Where("selectedNodes.Split(splitTerm, splitOptions).Length == 3", paramVals)
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.AreEqual(4444, result.Id);
		}

        [Ignore("We are ignoring this test because currently our ExpressionParser class cannot deal with this... it needs some serious TLC but it is very complex.")]
		[Test]
		public void Complex_Linq()
		{
			var doc = GetDynamicNode(1173);

			var paramVals = new Dictionary<string, object> {{"splitTerm", new char[] {','}}, {"searchId", "1173"}};
			var result = doc.Ancestors().OrderBy("level")
				.Single()
				.Descendants()
				.Where("selectedNodes != null && selectedNodes != String.Empty && selectedNodes.Split(splitTerm).Contains(searchId)", paramVals)
				.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.AreEqual(4444, result.Id);	
		}

		[Test]
		public void Index()
		{
			var doc = GetDynamicNode(1173);
			Assert.AreEqual(0, doc.Index());
			doc = GetDynamicNode(1176);
			Assert.AreEqual(3, doc.Index());
			doc = GetDynamicNode(1177);
			Assert.AreEqual(1, doc.Index());
			doc = GetDynamicNode(1178);
			Assert.AreEqual(2, doc.Index());
		}

		[Test]
		public virtual void Is_First_Root_Nodes()
		{
			var doc = GetDynamicNode(1046); //test root nodes
			Assert.IsTrue(doc.IsFirst());
			doc = GetDynamicNode(1172);
		    Assert.IsFalse(doc.IsFirst());
		}

        [Test]
        public void Is_First()
        {
            var doc = GetDynamicNode(1173); //test normal nodes
            Assert.IsTrue(doc.IsFirst());
            doc = GetDynamicNode(1175);
            Assert.IsFalse(doc.IsFirst());
        }

        [Test]
        public virtual void Is_Not_First_Root_Nodes()
        {
            var doc = GetDynamicNode(1046); //test root nodes
            Assert.IsFalse(doc.IsNotFirst());
            doc = GetDynamicNode(1172);
            Assert.IsTrue(doc.IsNotFirst());
        }

		[Test]
		public void Is_Not_First()
		{		
			var doc = GetDynamicNode(1173); //test normal nodes
			Assert.IsFalse(doc.IsNotFirst());
			doc = GetDynamicNode(1175);
			Assert.IsTrue(doc.IsNotFirst());
		}

        [Test]
        public virtual void Is_Position_Root_Nodes()
        {
            var doc = GetDynamicNode(1046); //test root nodes
            Assert.IsTrue(doc.IsPosition(0));
            doc = GetDynamicNode(1172);
            Assert.IsTrue(doc.IsPosition(1));            
        }

		[Test]
		public void Is_Position()
		{
			var doc = GetDynamicNode(1173); //test normal nodes
			Assert.IsTrue(doc.IsPosition(0));
			doc = GetDynamicNode(1175);
			Assert.IsTrue(doc.IsPosition(1));
		}

		[Test]
		public void Children_GroupBy_DocumentTypeAlias()
		{
			var doc = GetDynamicNode(1046);

			var found1 = doc.Children.GroupBy("DocumentTypeAlias");

			var casted = (IEnumerable<IGrouping<object, dynamic>>)(found1);
			Assert.AreEqual(2, casted.Count());
			Assert.AreEqual(2, casted.Single(x => x.Key.ToString() == "Home").Count());
			Assert.AreEqual(1, casted.Single(x => x.Key.ToString() == "CustomDocument").Count());
		}

		[Test]
		public void Children_Where_DocumentTypeAlias()
		{
			var doc = GetDynamicNode(1046);

			var found1 = doc.Children.Where("DocumentTypeAlias == \"CustomDocument\"");
			var found2 = doc.Children.Where("DocumentTypeAlias == \"Home\"");

			Assert.AreEqual(1, found1.Count());
			Assert.AreEqual(2, found2.Count());
		}

		[Test]
		public void Children_Where_NodeTypeAlias()
		{
			var doc = GetDynamicNode(1046);

			var found1 = doc.Children.Where("NodeTypeAlias == \"CustomDocument\"");
			var found2 = doc.Children.Where("NodeTypeAlias == \"Home\"");

			Assert.AreEqual(1, found1.Count());
			Assert.AreEqual(2, found2.Count());
		}

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
		public void Children_Order_By_Update_Date_Descending()
		{
			var asDynamic = GetDynamicNode(1173);

			var ordered = asDynamic.Children.OrderBy("UpdateDate desc");
			var casted = (IEnumerable<TDocument>)ordered;

			var correctOrder = new[] { 1176, 1174, 1177, 1178 };
			for (var i = 0; i < correctOrder.Length; i++)
			{
				Assert.AreEqual(correctOrder[i], ((dynamic)casted.ElementAt(i)).Id);
			}

		}

		[Test]
		public void HasProperty()
		{
			var asDynamic = GetDynamicNode(1173);

			var hasProp = asDynamic.HasProperty(Constants.Conventions.Content.UrlAlias);

			Assert.AreEqual(true, (bool)hasProp);

		}

		[Test]
		public void Skip()
		{
			var asDynamic = GetDynamicNode(1173);

			var skip = asDynamic.Children.Skip(2);
			var casted = (IEnumerable<TDocument>)skip;

			Assert.AreEqual(2, casted.Count());
		    Assert.IsTrue(casted.Select(x => ((dynamic) x).Id).ContainsAll(new dynamic[] {1178, 1176}));

		}

		[Test]
		public void HasValue()
		{
			var asDynamic = GetDynamicNode(1173);

			var hasValue = asDynamic.HasValue(Constants.Conventions.Content.UrlAlias);
			var noValue = asDynamic.HasValue("blahblahblah");

			Assert.IsTrue(hasValue);
			Assert.IsFalse(noValue);
		}

		[Test]
		public void Take()
		{
			var asDynamic = GetDynamicNode(1173);
			
			var take = asDynamic.Children.Take(2);
			var casted = (IEnumerable<TDocument>)take;

			Assert.AreEqual(2, casted.Count());
			Assert.IsTrue(casted.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1174, 1177 }));
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

            // ancestor-or-self has to be self!
            // but that's not what the "legacy" razor macro engine does...
            if (result is Umbraco.Web.Models.DynamicPublishedContent)
    			Assert.AreEqual(1173, (int)result.Id); // that one works
            else
                Assert.AreEqual(1046, (int)result.Id); // that one still is fubar
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
			Assert.AreEqual(9, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1046, 1173, 1174, 1176, 1175, 4444 }));
		}

		[Test]
		public void Descendants()
		{
			var asDynamic = GetDynamicNode(1046);

			var result = asDynamic.Descendants();

			Assert.IsNotNull(result);

			var list = (IEnumerable<TDocument>)result;
			Assert.AreEqual(8, list.Count());
			Assert.IsTrue(list.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1173, 1174, 1176, 1175, 4444 }));
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
            var asDynamic = GetDynamicNode(1176);

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

            Assert.AreEqual((int)1178, (int)result.Id);
		}
	}

	/// <summary>
	/// Extension methods used in tests
	/// </summary>
	public static class TestExtensionMethods
	{
		public static bool ContainsValue(this string s, int val)
		{
			return s.Contains(val.ToString());
		}
	}
}