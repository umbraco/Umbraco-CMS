using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Web.Routing;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Stubs;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Use this abstract class for tests that requires a Sql Ce database populated with the umbraco db schema.
    /// The PetaPoco Database class should be used through the <see cref="DatabaseFactory"/> singleton.
    /// </summary>
    [TestFixture, RequiresSTA]
    public abstract class BaseDatabaseFactoryTest
    {
        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            UmbracoSettings.UseLegacyXmlSchema = false;

			RepositoryResolver.Current = new RepositoryResolver(
				new RepositoryFactory());

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];
            ConfigurationManager.AppSettings.Set("umbracoDbDSN", @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\UmbracoPetaPocoTests.sdf");
            
            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            Resolution.Freeze();
            ApplicationContext = new ApplicationContext() { IsReady = true };
            DatabaseContext = DatabaseContext.Current;
            ServiceContext = ServiceContext.Current;

            //Configure the Database and Sql Syntax based on connection string set in config
            DatabaseContext.Initialize();
            //Create the umbraco database and its base data
            DatabaseContext.Database.CreateDatabaseSchema();
        }


        [TearDown]
        public virtual void TearDown()
        {
            TestHelper.CleanContentDirectories();

            //reset the app context
            DatabaseContext = null;
            ApplicationContext.Current = null;
            ServiceContext = null;
            Resolution.IsFrozen = false;

            RepositoryResolver.Reset();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                //File.Delete(filePath);
            }
        }

        protected ApplicationContext ApplicationContext { get; set; }

        protected ServiceContext ServiceContext { get; set; }

        protected DatabaseContext DatabaseContext { get; set; }

        protected UmbracoContext GetUmbracoContext(string url, int templateId, RouteData routeData = null)
        {
            var ctx = new UmbracoContext(
                GetHttpContextFactory(url, routeData).HttpContext,
                ApplicationContext,
                GetRoutesCache());
            SetupUmbracoContextForTest(ctx, templateId);
            return ctx;
        }

        protected FakeHttpContextFactory GetHttpContextFactory(string url, RouteData routeData = null)
        {
            var factory = routeData != null
                            ? new FakeHttpContextFactory(url, routeData)
                            : new FakeHttpContextFactory(url);


            //set the state helper
            StateHelper.HttpContext = factory.HttpContext;

            return factory;
        }

        internal virtual IRoutesCache GetRoutesCache()
        {
            return new FakeRoutesCache();
        }

        /// <summary>
        /// Initlializes the UmbracoContext with specific XML
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="templateId"></param>
        protected void SetupUmbracoContextForTest(UmbracoContext umbracoContext, int templateId)
        {
            umbracoContext.GetXmlDelegate = () =>
            {
                var xDoc = new XmlDocument();

                //create a custom xml structure to return

                xDoc.LoadXml(GetXmlContent(templateId));
                //return the custom x doc
                return xDoc;
            };
        }

        protected virtual string GetXmlContent(int templateId)
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
		<Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
			<content><![CDATA[<div>This is some content</div>]]></content>
			<umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>			
			<Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
				<content><![CDATA[]]></content>
				<umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
				<creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
			</Home>
			<Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""">
				<content><![CDATA[]]></content>
			</Home>
			<CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
			<CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""" />
		</Home>
		<Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
		</Home>
	</Home>
	<CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        }
    }
}