using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Xml;
using NUnit.Framework;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.Stubs;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Use this abstract class for tests that requires a Sql Ce database populated with the umbraco db schema.
    /// The PetaPoco Database class should be used through the <see cref="DefaultDatabaseFactory"/>.
    /// </summary>
    [TestFixture, RequiresSTA]
    public abstract class BaseDatabaseFactoryTest : BaseUmbracoApplicationTest
    {
        //This is used to indicate that this is the first test to run in the test session, if so, we always
        //ensure a new database file is used.
        private static volatile bool _firstRunInTestSession = true;
        private static readonly object Locker = new object();
        private bool _firstTestInFixture = true;

        //Used to flag if its the first test in the current session
        private bool _isFirstRunInTestSession = false;
        //Used to flag if its the first test in the current fixture
        private bool _isFirstTestInFixture = false;

        private ApplicationContext _appContext;

        [SetUp]
        public override void Initialize()
        {
            InitializeFirstRunFlags();
            
            var path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            var dbFactory = new DefaultDatabaseFactory(
                GetDbConnectionString(),
                GetDbProviderName());
            _appContext = new ApplicationContext(
				//assign the db context
                new DatabaseContext(dbFactory),
				//assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(dbFactory), new FileUnitOfWorkProvider(), new PublishingStrategy()),
                //disable cache
                false)
                {
                    IsReady = true
                };

            base.Initialize();

            DatabaseContext.Initialize(dbFactory.ProviderName, dbFactory.ConnectionString);

            CreateSqlCeDatabase();

            InitializeDatabase();

            //ensure the configuration matches the current version for tests
            SettingsForTests.ConfigurationStatus = UmbracoVersion.Current.ToString(3);
        }

        protected override void SetupApplicationContext()
        {
            ApplicationContext.Current = _appContext;
        }

        /// <summary>
        /// The database behavior to use for the test/fixture
        /// </summary>
        protected virtual DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerTest; }
        }

        protected virtual string GetDbProviderName()
        {
            return "System.Data.SqlServerCe.4.0";
        }

        /// <summary>
        /// Get the db conn string
        /// </summary>
        protected virtual string GetDbConnectionString()
        {
            return @"Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;";            
        }

        /// <summary>
        /// Creates the SqlCe database if required
        /// </summary>
        protected virtual void CreateSqlCeDatabase()
        {
            if (DatabaseTestBehavior == DatabaseBehavior.NoDatabasePerFixture)
                return;

            var path = TestHelper.CurrentAssemblyDirectory;
            
            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            ConfigurationManager.AppSettings.Set(
                Core.Configuration.GlobalSettings.UmbracoConnectionName, 
                GetDbConnectionString());

            string dbFilePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");

            //create a new database file if
            // - is the first test in the session
            // - the database file doesn't exist
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            //if this is the first test in the session, always ensure a new db file is created
            if (_isFirstRunInTestSession || !File.Exists(dbFilePath) 
                || DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest
                || (_isFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerFixture))
            {
                
                RemoveDatabaseFile(ex =>
                    {
                        //if this doesn't work we have to make sure everything is reset! otherwise
                        // well run into issues because we've already set some things up
                        TearDown();
                        throw ex;
                    });

                //Create the Sql CE database
                var engine = new SqlCeEngine(settings.ConnectionString);
                engine.CreateDatabase();
            }

            //clear the database if
            // - NewSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewSchemaPerFixture

            else if (DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerTest
                || (_isFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerFixture))
            {
                DatabaseContext.Database.UninstallDatabaseSchema();
            }
        }

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            DataTypesResolver.Current = new DataTypesResolver(
                () => PluginManager.Current.ResolveDataTypes());

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory());

            SqlSyntaxProvidersResolver.Current = new SqlSyntaxProvidersResolver(
                new List<Type> { typeof(MySqlSyntaxProvider), typeof(SqlCeSyntaxProvider), typeof(SqlServerSyntaxProvider) }) { CanResolveBeforeFrozen = true };

            MappingResolver.Current = new MappingResolver(
               () => PluginManager.Current.ResolveAssignedMapperTypes());

            base.FreezeResolution();
        }

        /// <summary>
        /// Creates the tables and data for the database
        /// </summary>
        protected virtual void InitializeDatabase()
        {
            if (DatabaseTestBehavior == DatabaseBehavior.NoDatabasePerFixture)
                return;

            //create the schema and load default data if:
            // - is the first test in the session
            // - NewSchemaPerTest
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewSchemaPerFixture
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            if (_isFirstRunInTestSession
                || DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerTest
                || (_isFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerFixture))
            {
                //Create the umbraco database and its base data
                DatabaseContext.Database.CreateDatabaseSchema(false);
            }            
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerFixture)
            {
                RemoveDatabaseFile();
            }
        }

        [TearDown]
        public override void TearDown()
        {
            _isFirstTestInFixture = false; //ensure this is false before anything!

            if (DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest)
            {
                RemoveDatabaseFile();
            }
            else if (DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerTest)
            {
                DatabaseContext.Database.UninstallDatabaseSchema();
            }           
           
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            SqlSyntaxContext.SqlSyntaxProvider = null;

            base.TearDown();
        }

        private void CloseDbConnections()
        {
            //Ensure that any database connections from a previous test is disposed. 
            //This is really just double safety as its also done in the TearDown.
            if (ApplicationContext != null && DatabaseContext != null && DatabaseContext.Database != null)
                DatabaseContext.Database.Dispose();
            SqlCeContextGuardian.CloseBackgroundConnection();
        }

        private void InitializeFirstRunFlags()
        {
            //this needs to be thread-safe
            _isFirstRunInTestSession = false;
            if (_firstRunInTestSession)
            {
                lock (Locker)
                {
                    if (_firstRunInTestSession)
                    {
                        _isFirstRunInTestSession = true; //set the flag
                        _firstRunInTestSession = false;
                    }
                }
            }
            if (_firstTestInFixture)
            {
                lock (Locker)
                {
                    if (_firstTestInFixture)
                    {
                        _isFirstTestInFixture = true; //set the flag
                        _firstTestInFixture = false;
                    }
                }
            }
        }

        private void RemoveDatabaseFile(Action<Exception> onFail = null)
        {
            CloseDbConnections();
            string path = TestHelper.CurrentAssemblyDirectory;
            try
            {
                string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<BaseDatabaseFactoryTest>("Could not remove the old database file", ex);

                //We will swallow this exception! That's because a sub class might require further teardown logic.
                if (onFail != null)
                {
                    onFail(ex);
                }
            }
        }

	    protected ServiceContext ServiceContext
	    {
		    get { return ApplicationContext.Services; }
	    }

	    protected DatabaseContext DatabaseContext
	    {
		    get { return ApplicationContext.DatabaseContext; }
	    }

        protected UmbracoContext GetUmbracoContext(string url, int templateId, RouteData routeData = null, bool setSingleton = false)
        {
            var cache = new PublishedContentCache();

            cache.GetXmlDelegate = (context, preview) =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(GetXmlContent(templateId));
                    return doc;
                };

            PublishedContentCache.UnitTesting = true;

            var ctx = new UmbracoContext(
                GetHttpContextFactory(url, routeData).HttpContext,
                ApplicationContext,
                new PublishedCaches(cache, new PublishedMediaCache()));

            if (setSingleton)
            {
                UmbracoContext.Current = ctx;
            }

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