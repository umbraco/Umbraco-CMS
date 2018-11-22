using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Web.Routing;
using System.Xml;
using NUnit.Framework;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for Umbraco application tests that require a database.
    /// </summary>
    /// <remarks>Can provide a SqlCE database populated with the Umbraco schema. The database should be accessed
    /// through the <see cref="DefaultDatabaseFactory"/>.</remarks>
    [TestFixture, RequiresSTA]
    public abstract class BaseDatabaseFactoryTest : BaseUmbracoApplicationTest
    {
        //This is used to indicate that this is the first test to run in the test session, if so, we always
        //ensure a new database file is used.
        private static volatile bool _firstRunInTestSession = true;
        private static readonly object Locker = new object();
        private bool _firstTestInFixture = true;

        //Used to flag if its the first test in the current session
        private bool _isFirstRunInTestSession;
        //Used to flag if its the first test in the current fixture
        private bool _isFirstTestInFixture;

        private ApplicationContext _appContext;

        private string _dbPath;
        //used to store (globally) the pre-built db with schema and initial data
        private static byte[] _dbBytes;
        private DefaultDatabaseFactory _dbFactory;

        [SetUp]
        public override void Initialize()
        {
            InitializeFirstRunFlags();

            var path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            // we probably don't need this here, as it's done in base.Initialize() already,
            // but these test classes are all weird, not going to change it now - v8
            SafeCallContext.Clear();

            _dbFactory = new DefaultDatabaseFactory(
                GetDbConnectionString(),
                GetDbProviderName(),
                Logger);

            // ensure we start tests in a clean state ie without any scope in context
            // anything that used a true 'Scope' would have removed it, but there could
            // be a rogue 'NoScope' there - and we want to make sure it is gone
            var scopeProvider = new ScopeProvider(null);
            if (scopeProvider.AmbientScope != null)
                scopeProvider.AmbientScope.Dispose(); // removes scope from context

            base.Initialize();

            using (ProfilingLogger.TraceDuration<BaseDatabaseFactoryTest>("init"))
            {
                //TODO: Somehow make this faster - takes 5s +

                DatabaseContext.Initialize(_dbFactory.ProviderName, _dbFactory.ConnectionString);
                CreateSqlCeDatabase();
                InitializeDatabase();

                //ensure the configuration matches the current version for tests
                SettingsForTests.ConfigurationStatus = UmbracoVersion.GetSemanticVersion().ToSemanticString();
            }
        }

        protected override ApplicationContext CreateApplicationContext()
        {
            var repositoryFactory = new RepositoryFactory(CacheHelper, Logger, SqlSyntax, SettingsForTests.GenerateMockSettings());

            var evtMsgs = new TransientMessagesFactory();
            var scopeProvider = new ScopeProvider(_dbFactory);
            _appContext = new ApplicationContext(
                //assign the db context
                new DatabaseContext(scopeProvider, Logger, SqlSyntax, GetDbProviderName()),
                //assign the service context
                new ServiceContext(repositoryFactory, new PetaPocoUnitOfWorkProvider(scopeProvider), CacheHelper, Logger, evtMsgs),
                CacheHelper,
                ProfilingLogger)
            {
                IsReady = true
            };

            return _appContext;
        }

        protected virtual ISqlSyntaxProvider SqlSyntax
        {
            get { return GetSyntaxProvider(); }
        }

        /// <summary>
        /// The database behavior to use for the test/fixture
        /// </summary>
        protected DatabaseBehavior DatabaseTestBehavior
        {
            get
            {
                var att = GetType().GetCustomAttribute<DatabaseTestBehaviorAttribute>(false);
                return att != null ? att.Behavior : DatabaseBehavior.NoDatabasePerFixture;
            }
        }

        protected virtual ISqlSyntaxProvider GetSyntaxProvider()
        {
            return new SqlCeSyntaxProvider();
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
            var settings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
            ConfigurationManager.AppSettings.Set(
                Constants.System.UmbracoConnectionName,
                GetDbConnectionString());

            _dbPath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");

            //create a new database file if
            // - is the first test in the session
            // - the database file doesn't exist
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            //if this is the first test in the session, always ensure a new db file is created
            if (_isFirstRunInTestSession || File.Exists(_dbPath) == false
                || (DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest || DatabaseTestBehavior == DatabaseBehavior.EmptyDbFilePerTest)
                || (_isFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerFixture))
            {

                using (ProfilingLogger.TraceDuration<BaseDatabaseFactoryTest>("Remove database file"))
                {
                    RemoveDatabaseFile(ex =>
                    {
                        //if this doesn't work we have to make sure everything is reset! otherwise
                        // well run into issues because we've already set some things up
                        TearDown();
                        throw ex;
                    });
                }

                //Create the Sql CE database
                using (ProfilingLogger.TraceDuration<BaseDatabaseFactoryTest>("Create database file"))
                {
                    if (DatabaseTestBehavior != DatabaseBehavior.EmptyDbFilePerTest && _dbBytes != null)
                    {
                        File.WriteAllBytes(_dbPath, _dbBytes);
                    }
                    else
                    {
                        using (var engine = new SqlCeEngine(settings.ConnectionString))
                        {
                            engine.CreateDatabase();
                        }
                    }
                }

            }

        }

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            PropertyEditorResolver.Current = new PropertyEditorResolver(
                new ActivatorServiceProvider(), Logger,
                () => PluginManager.Current.ResolvePropertyEditors(),
                ApplicationContext.ApplicationCache.RuntimeCache);

            DataTypesResolver.Current = new DataTypesResolver(
                new ActivatorServiceProvider(), Logger,
                () => PluginManager.Current.ResolveDataTypes());

            MappingResolver.Current = new MappingResolver(
                new ActivatorServiceProvider(), Logger,
               () => PluginManager.Current.ResolveAssignedMapperTypes());

            if (PropertyValueConvertersResolver.HasCurrent == false)
                PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver(new ActivatorServiceProvider(), Logger);

            if (PublishedContentModelFactoryResolver.HasCurrent == false)
                PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();

            base.FreezeResolution();
        }

        /// <summary>
        /// Creates the tables and data for the database
        /// </summary>
        protected virtual void InitializeDatabase()
        {
            if (DatabaseTestBehavior == DatabaseBehavior.NoDatabasePerFixture || DatabaseTestBehavior == DatabaseBehavior.EmptyDbFilePerTest)
                return;

            //create the schema and load default data if:
            // - is the first test in the session
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            if (_dbBytes == null &&
                (_isFirstRunInTestSession
                || DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest
                || (_isFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerFixture)))
            {

                var schemaHelper = new DatabaseSchemaHelper(DatabaseContext.Database, Logger, SqlSyntax);
                //Create the umbraco database and its base data
                schemaHelper.CreateDatabaseSchema(false, ApplicationContext);

                //close the connections, we're gonna read this baby in as a byte array so we don't have to re-initialize the
                // damn db for each test
                CloseDbConnections();

                _dbBytes = File.ReadAllBytes(_dbPath);
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            RemoveDatabaseFile();
        }

        [TearDown]
        public override void TearDown()
        {
            using (ProfilingLogger.TraceDuration<BaseDatabaseFactoryTest>("teardown"))
            {
                _isFirstTestInFixture = false; //ensure this is false before anything!

                if (DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest)
                    RemoveDatabaseFile(); // closes connections too
                else
                    CloseDbConnections();

                AppDomain.CurrentDomain.SetData("DataDirectory", null);

                SqlSyntaxContext.SqlSyntaxProvider = null;
            }

            base.TearDown();
        }

        private void CloseDbConnections()
        {
            // just to be sure, although it's also done in TearDown
            if (ApplicationContext != null
                && ApplicationContext.DatabaseContext != null
                && ApplicationContext.DatabaseContext.ScopeProvider != null)
            {
                ApplicationContext.DatabaseContext.ScopeProvider.Reset();
            }

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
            if (_firstTestInFixture == false) return;

            lock (Locker)
            {
                if (_firstTestInFixture == false) return;

                _isFirstTestInFixture = true; //set the flag
                _firstTestInFixture = false;
            }
        }

        private void RemoveDatabaseFile(Action<Exception> onFail = null)
        {
            CloseDbConnections();
            var path = TestHelper.CurrentAssemblyDirectory;
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
                    onFail(ex);
            }
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

            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;
            var ctx = new UmbracoContext(
                httpContext,
                ApplicationContext,
                new PublishedCaches(cache, new PublishedMediaCache(ApplicationContext)),
                new WebSecurity(httpContext, ApplicationContext));

            if (setSingleton)
            {
                UmbracoContext.Current = ctx;
            }

            return ctx;
        }

        protected virtual FakeHttpContextFactory GetHttpContextFactory(string url, RouteData routeData = null)
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