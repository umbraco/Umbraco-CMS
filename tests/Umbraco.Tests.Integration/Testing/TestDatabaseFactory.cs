using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class TestDatabaseFactory
{
    private readonly TestUmbracoDatabaseFactoryProvider _testUmbracoDatabaseFactoryProvider;
    private readonly IDatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
    private readonly IDatabaseDataCreator _databaseDataCreator;
    private const string WithSchemaKey = "bd8b3372-f88f-4d08-b341-6bb22b1d0eac";
    private const string WithoutSchemaKey = "90dde93b-7108-4a6d-b223-acd0d07df4a8";
    private const string InstanceName = "UmbracoIntegrationTest";
    private LocalDb _localDb;
    private static LocalDb.Instance s_localDbInstance;
    private static string s_filesPath;
    private readonly TestHelper _testHelper = new();
    private bool _initialized;
    private IOptionsMonitor<ConnectionStrings> _connectionStrings;
    public TestDatabaseFactory(TestUmbracoDatabaseFactoryProvider testUmbracoDatabaseFactoryProvider, IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory, IDatabaseDataCreator databaseDataCreator, IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _testUmbracoDatabaseFactoryProvider = testUmbracoDatabaseFactoryProvider;
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        _databaseDataCreator = databaseDataCreator;
        _connectionStrings = connectionStrings;
        _localDb = new LocalDb();

        s_filesPath = Path.Combine(_testHelper.WorkingDirectory, "databases");

        if (!Directory.Exists(s_filesPath))
        {
            Directory.CreateDirectory(s_filesPath);
        }

        s_localDbInstance = _localDb.GetInstance(InstanceName);
        if (s_localDbInstance != null)
        {
            return;
        }

        if (_localDb.CreateInstance(InstanceName) == false)
        {
            throw new Exception("Failed to create a LocalDb instance.");
        }

        // It looks wierd that we call this twice, but its the only way to get the database after it has been created.
        s_localDbInstance = _localDb.GetInstance(InstanceName);
    }

    public TestDatabaseMeta InitializeWithSchema()
    {
        EnsureInitialized();
        var key = Guid.NewGuid();
        
            try
            {
                _localDb.CopyDatabaseFiles(WithSchemaKey, s_filesPath, key.ToString());
                s_localDbInstance.AttachDatabase(key.ToString(), s_filesPath);
            }
            catch (Exception e)
            {
                var myKey = Guid.NewGuid().ToString();
                s_localDbInstance.CreateDatabase(myKey, s_filesPath);
                var umbracoConnectionString = new ConnectionStrings
                {
                    ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, myKey),
                    ProviderName = "Microsoft.Data.SqlClient",
                };
                umbracoConnectionString.ConnectionString += "TrustServerCertificate=true;";
                _connectionStrings.CurrentValue.ConnectionString = umbracoConnectionString.ConnectionString;
                _connectionStrings.CurrentValue.ProviderName = umbracoConnectionString.ProviderName;

                var dbFactory = _testUmbracoDatabaseFactoryProvider.Create();
                dbFactory.Configure(umbracoConnectionString);
                var db = dbFactory.CreateDatabase();
                db.BeginTransaction();
                IDatabaseSchemaCreator creator = _databaseSchemaCreatorFactory.Create(db);
                creator.InitializeDatabaseSchema(false).GetAwaiter().GetResult();
                db.CompleteTransaction();
                _databaseDataCreator.SeedDataAsync().GetAwaiter().GetResult();
                
                return new TestDatabaseMeta()
                {
                    ConnectionStrings = umbracoConnectionString,
                    Key = myKey,
                };
            }


            var connectionStrings = new ConnectionStrings
        {
            ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, key.ToString()),
            ProviderName = "Microsoft.Data.SqlClient",
        };

        connectionStrings.ConnectionString += "TrustServerCertificate=true;";
        
        return new TestDatabaseMeta()
        {
            ConnectionStrings = connectionStrings,
            Key = key.ToString(),
        };
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        var filePath = Path.Combine(s_filesPath, WithoutSchemaKey + ".mdf");
        if (!File.Exists(filePath))
        {
            s_localDbInstance.CreateDatabase(WithoutSchemaKey, s_filesPath);
            var umbracoConnectionString = new ConnectionStrings
            {
                ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, WithoutSchemaKey),
                ProviderName = "Microsoft.Data.SqlClient",
            };
            umbracoConnectionString.ConnectionString += "TrustServerCertificate=true;";
            _connectionStrings.CurrentValue.ConnectionString = umbracoConnectionString.ConnectionString;
            _connectionStrings.CurrentValue.ProviderName = umbracoConnectionString.ProviderName;
        }

        if (!File.Exists(Path.Combine(s_filesPath, WithSchemaKey + ".mdf")))
        {
            s_localDbInstance.CreateDatabase(WithSchemaKey, s_filesPath);
            var umbracoConnectionString = new ConnectionStrings
            {
                ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, WithSchemaKey),
                ProviderName = "Microsoft.Data.SqlClient",
            };
            umbracoConnectionString.ConnectionString += "TrustServerCertificate=true;";
            _connectionStrings.CurrentValue.ConnectionString = umbracoConnectionString.ConnectionString;
            _connectionStrings.CurrentValue.ProviderName = umbracoConnectionString.ProviderName;

            var dbFactory = _testUmbracoDatabaseFactoryProvider.Create();
            dbFactory.Configure(umbracoConnectionString);
            var db = dbFactory.CreateDatabase();
            db.BeginTransaction();
            IDatabaseSchemaCreator creator = _databaseSchemaCreatorFactory.Create(db);
            creator.InitializeDatabaseSchema(false).GetAwaiter().GetResult();
            db.CompleteTransaction();
            _databaseDataCreator.SeedDataAsync().GetAwaiter().GetResult();
        }

        _initialized = true;
    }

    public void Teardown(string key)
    {
        s_localDbInstance.DropDatabase(key);
    }
}

public class TestDatabaseMeta
{
    public ConnectionStrings ConnectionStrings { get; set; }
    public string Key { get; set; }
}

