using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading;
using System.Web.Routing;
using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Security;
using Umbraco.Web.Routing;
using File = System.IO.File;
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Testing;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.Testing.Objects.Accessors;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for all Umbraco tests that require a database.
    /// </summary>
    /// <remarks>
    /// <para>Can provide a SqlCE database populated with the Umbraco schema. The database should
    /// be accessed through the UmbracoDatabaseFactory.</para>
    /// <para>Provides an Umbraco context and Xml content.</para>
    /// <para>fixme what else?</para>
    /// </remarks>
    [Apartment(ApartmentState.STA)] // why?
    [UmbracoTest(WithApplication = true)]
    public abstract class TestWithDatabaseBase : UmbracoTestBase
    {
        private string _databasePath;
        private static byte[] _databaseBytes;

        protected PublishedContentTypeCache ContentTypesCache { get; private set; }

        protected override ISqlSyntaxProvider SqlSyntax => GetSyntaxProvider();

        protected ServiceContext ServiceContext => Current.Services;

        internal ScopeProvider ScopeProvider => Current.ScopeProvider as ScopeProvider;

        protected ISqlContext SqlContext => Factory.GetInstance<ISqlContext>();

        public override void SetUp()
        {
            base.SetUp();

            var path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }

        protected override void Compose()
        {
            base.Compose();

            Composition.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>();
            Composition.Register(factory => PublishedSnapshotService);
            Composition.Register(factory => DefaultCultureAccessor);

            Composition.WithCollectionBuilder<DataEditorCollectionBuilder>()
                .Clear()
                .Add(() => Composition.TypeLoader.GetDataEditors());

            Composition.RegisterUnique(f =>
            {
                if (Options.Database == UmbracoTestOptions.Database.None)
                    return TestObjects.GetDatabaseFactoryMock();

                var logger = f.GetInstance<ILogger>();
                var mappers = f.GetInstance<IMapperCollection>();
                var factory = new UmbracoDatabaseFactory(GetDbConnectionString(), GetDbProviderName(), logger, new Lazy<IMapperCollection>(() => mappers));
                factory.ResetForTests();
                return factory;
            });
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            RemoveDatabaseFile();
        }

        public override void TearDown()
        {
            var profilingLogger = Factory.TryGetInstance<IProfilingLogger>();
            var timer = profilingLogger?.TraceDuration<TestWithDatabaseBase>("teardown"); // FIXME: move that one up
            try
            {
                // FIXME: should we first kill all scopes?
                if (Options.Database == UmbracoTestOptions.Database.NewSchemaPerTest)
                    RemoveDatabaseFile();

                AppDomain.CurrentDomain.SetData("DataDirectory", null);

                // make sure we dispose of the service to unbind events
                PublishedSnapshotService?.Dispose();
                PublishedSnapshotService = null;
            }
            finally
            {
                timer?.Dispose();
            }

            base.TearDown();
        }

        private void CreateAndInitializeDatabase()
        {
            using (ProfilingLogger.TraceDuration<TestWithDatabaseBase>("Create database."))
            {
                CreateSqlCeDatabase(); // TODO: faster!
            }

            // ensure the configuration matches the current version for tests
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.ConfigurationStatus).Returns(UmbracoVersion.Current.ToString(3));

            using (ProfilingLogger.TraceDuration<TestWithDatabaseBase>("Initialize database."))
            {
                InitializeDatabase(); // TODO: faster!
            }
        }

        protected virtual ISqlSyntaxProvider GetSyntaxProvider()
        {
            return new SqlCeSyntaxProvider();
        }

        protected virtual string GetDbProviderName()
        {
            return Constants.DbProviderNames.SqlCe;
        }

        protected virtual string GetDbConnectionString()
        {
            return @"Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;";
        }

        protected FakeHttpContextFactory GetHttpContextFactory(string url, RouteData routeData = null)
        {
            var factory = routeData != null
                            ? new FakeHttpContextFactory(url, routeData)
                            : new FakeHttpContextFactory(url);

            return factory;
        }

        /// <summary>
        /// Creates the SqlCe database if required
        /// </summary>
        protected virtual void CreateSqlCeDatabase()
        {
            if (Options.Database == UmbracoTestOptions.Database.None)
                return;

            var path = TestHelper.CurrentAssemblyDirectory;

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
            ConfigurationManager.AppSettings.Set(
                Constants.System.UmbracoConnectionName,
                GetDbConnectionString());

            _databasePath = string.Concat(path, "\\UmbracoNPocoTests.sdf");

            //create a new database file if
            // - is the first test in the session
            // - the database file doesn't exist
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            //if this is the first test in the session, always ensure a new db file is created
            if (FirstTestInSession
                || File.Exists(_databasePath) == false
                || Options.Database == UmbracoTestOptions.Database.NewSchemaPerTest
                || Options.Database == UmbracoTestOptions.Database.NewEmptyPerTest
                || (FirstTestInFixture && Options.Database == UmbracoTestOptions.Database.NewSchemaPerFixture))
            {
                using (ProfilingLogger.TraceDuration<TestWithDatabaseBase>("Remove database file"))
                {
                    RemoveDatabaseFile(null, ex =>
                    {
                        //if this doesn't work we have to make sure everything is reset! otherwise
                        // well run into issues because we've already set some things up
                        TearDown();
                        throw ex;
                    });
                }

                //Create the Sql CE database
                using (ProfilingLogger.TraceDuration<TestWithDatabaseBase>("Create database file"))
                {
                    if (Options.Database != UmbracoTestOptions.Database.NewEmptyPerTest && _databaseBytes != null)
                    {
                        File.WriteAllBytes(_databasePath, _databaseBytes);
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

        protected IDefaultCultureAccessor DefaultCultureAccessor { get; set; }

        protected IPublishedSnapshotService PublishedSnapshotService { get; set; }

        protected override void Initialize() // FIXME: should NOT be here!
        {
            base.Initialize();

            DefaultCultureAccessor = new TestDefaultCultureAccessor();

            CreateAndInitializeDatabase();

            // ensure we have a PublishedSnapshotService
            if (PublishedSnapshotService == null)
            {
                PublishedSnapshotService = CreatePublishedSnapshotService();
            }
        }

        protected virtual IPublishedSnapshotService CreatePublishedSnapshotService()
        {
            var cache = NoAppCache.Instance;

            ContentTypesCache = new PublishedContentTypeCache(
                Factory.GetInstance<IContentTypeService>(),
                Factory.GetInstance<IMediaTypeService>(),
                Factory.GetInstance<IMemberTypeService>(),
                Factory.GetInstance<IPublishedContentTypeFactory>(),
                Logger);

            // testing=true so XmlStore will not use the file nor the database

            var publishedSnapshotAccessor = new UmbracoContextPublishedSnapshotAccessor(Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            var variationContextAccessor = new TestVariationContextAccessor();
            var service = new PublishedSnapshotService(
                ServiceContext,
                Factory.GetInstance<IPublishedContentTypeFactory>(),
                ScopeProvider,
                cache, publishedSnapshotAccessor, variationContextAccessor,
                Factory.GetInstance<IDocumentRepository>(), Factory.GetInstance<IMediaRepository>(), Factory.GetInstance<IMemberRepository>(),
                DefaultCultureAccessor,
                Logger,
                Factory.GetInstance<IGlobalSettings>(), new SiteDomainHelper(),
                Factory.GetInstance<IEntityXmlSerializer>(),
                ContentTypesCache,
                null, true, Options.PublishedRepositoryEvents);

            // initialize PublishedCacheService content with an Xml source
            service.XmlStore.GetXmlDocument = () =>
            {
                var doc = new XmlDocument();
                doc.LoadXml(GetXmlContent(0));
                return doc;
            };

            return service;
        }

        /// <summary>
        /// Creates the tables and data for the database
        /// </summary>
        protected virtual void InitializeDatabase()
        {
            if (Options.Database == UmbracoTestOptions.Database.None || Options.Database == UmbracoTestOptions.Database.NewEmptyPerTest)
                return;

            //create the schema and load default data if:
            // - is the first test in the session
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            if (_databaseBytes == null &&
                (FirstTestInSession
                || Options.Database == UmbracoTestOptions.Database.NewSchemaPerTest
                || FirstTestInFixture && Options.Database == UmbracoTestOptions.Database.NewSchemaPerFixture))
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    var schemaHelper = new DatabaseSchemaCreator(scope.Database, Logger);
                    //Create the umbraco database and its base data
                    schemaHelper.InitializeDatabaseSchema();
                    scope.Complete();
                }

                _databaseBytes = File.ReadAllBytes(_databasePath);
            }
        }

        // FIXME: is this needed?
        private void CloseDbConnections(IUmbracoDatabase database)
        {
            //Ensure that any database connections from a previous test is disposed.
            //This is really just double safety as its also done in the TearDown.
            database?.Dispose();
            //SqlCeContextGuardian.CloseBackgroundConnection();
        }

        private void RemoveDatabaseFile(IUmbracoDatabase database, Action<Exception> onFail = null)
        {
            if (database != null) CloseDbConnections(database);
            RemoveDatabaseFile(onFail);
        }

        private void RemoveDatabaseFile(Action<Exception> onFail = null)
        {
            var path = TestHelper.CurrentAssemblyDirectory;
            try
            {
                var filePath = string.Concat(path, "\\UmbracoNPocoTests.sdf");
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error<TestWithDatabaseBase>(ex, "Could not remove the old database file");

                // swallow this exception - that's because a sub class might require further teardown logic
                onFail?.Invoke(ex);
            }
        }

        protected UmbracoContext GetUmbracoContext(string url, int templateId = 1234, RouteData routeData = null, bool setSingleton = false, IUmbracoSettingsSection umbracoSettings = null, IEnumerable<IUrlProvider> urlProviders = null, IGlobalSettings globalSettings = null, IPublishedSnapshotService snapshotService = null)
        {
            // ensure we have a PublishedCachesService
            var service = snapshotService ?? PublishedSnapshotService as PublishedSnapshotService;
            if (service == null)
                throw new Exception("Not a proper XmlPublishedCache.PublishedCachesService.");

            if (service is PublishedSnapshotService)
            {
                // re-initialize PublishedCacheService content with an Xml source with proper template id
                ((PublishedSnapshotService)service).XmlStore.GetXmlDocument = () =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(GetXmlContent(templateId));
                    return doc;
                };
            }

            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;

            var umbracoContext = new UmbracoContext(
                httpContext,
                service,
                new WebSecurity(httpContext, Factory.GetInstance<IUserService>(),
                    Factory.GetInstance<IGlobalSettings>()),
                umbracoSettings ?? Factory.GetInstance<IUmbracoSettingsSection>(),
                urlProviders ?? Enumerable.Empty<IUrlProvider>(),
                globalSettings ?? Factory.GetInstance<IGlobalSettings>(),
                new TestVariationContextAccessor());

            if (setSingleton)
                Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;

            return umbracoContext;
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
