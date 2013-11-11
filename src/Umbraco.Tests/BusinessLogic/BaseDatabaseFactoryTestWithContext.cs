using System.Diagnostics;
using System.Reflection;
using System;
using Umbraco.Web;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers
{
    public abstract partial class BaseDatabaseFactoryTestWithContext : BaseDatabaseFactoryTest
    {
        protected abstract void EnsureData();

        public override void Initialize()
        {
            base.Initialize();
            if (!initialized) CreateContext(); 
            EnsureData();
        }

        protected bool initialized;

        protected UmbracoContext context;
        protected UmbracoDatabase database;

        protected void CreateContext()
        {
            context = GetUmbracoContext("http://localhost", 0);
            database = context.Application.DatabaseContext.Database;
        }


        private Umbraco.Core.Persistence.Database _independentDatabase;
        protected Umbraco.Core.Persistence.Database independentDatabase
        {
            get
            {
                if (_independentDatabase == null)
                {
                    if (database.Connection == null) database.OpenSharedConnection();

                    string connectionString = ReplaceDataDirectory(database.Connection.ConnectionString);
                    string providerName = "System.Data.SqlServerCe.4.0";
                    _independentDatabase = new Umbraco.Core.Persistence.Database(connectionString, providerName);

                }
                return _independentDatabase;
            }
        }

        private string ReplaceDataDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.Contains("|DataDirectory|"))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (!string.IsNullOrEmpty(dataDirectory))
                {
                    path = path.Contains(@"|\")
                        ? path.Replace("|DataDirectory|", dataDirectory)
                        : path.Replace("|DataDirectory|", dataDirectory + System.IO.Path.DirectorySeparatorChar);
                }
            }
            return path;
        }

        protected T getPersistedTestDto<T>(int id, string idKeyName = "id")
        {
            return independentDatabase.SingleOrDefault<T>(string.Format("where {0} = @0", idKeyName), id);
        }

        #region Helper methods
        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        protected void l(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        protected bool _traceTestCompletion = false;
        private int _testNumber;
        protected void traceCompletion(string finished = "Finished")
        {
            if (!_traceTestCompletion) return;
            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
            string message = string.Format("***** {0:000}. {1} - {2} *****\n", ++_testNumber, methodBase.Name, finished);
            System.Console.Write(message);
        }
        #endregion


    }
}