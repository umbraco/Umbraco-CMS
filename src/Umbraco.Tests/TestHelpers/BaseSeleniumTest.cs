using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Management;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.TestHelpers
{
    public class BaseSeleniumTest
    {
        internal IWebDriver Driver;
        internal string BaseUrl;

        private StringBuilder _verificationErrors;
        private UmbracoDatabase _database;

        protected ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }

        [SetUp]
        public virtual void Initialize()
        {

            // Disable medium trust
            var transform = TransformWebConfig("Release");

            var assemblyPath = TestHelper.CurrentAssemblyDirectory;
            assemblyPath = Path.Combine(assemblyPath, @"..\..\..\Umbraco.Web.UI\");
            var webUiPath = Path.GetFullPath(new Uri(assemblyPath).LocalPath);

            var installedPackagesConfig = string.Format("{0}App_Data\\packages\\installed\\installedPackages.config", webUiPath);
            if (File.Exists(installedPackagesConfig))
                File.Delete(installedPackagesConfig);

            var databaseDataPath = string.Format(@"{0}\App_Data\Umbraco.sdf", webUiPath);
            var connectionString = string.Format(@"Data Source={0}", databaseDataPath);

            //Create the Sql CE database
            var engine = new SqlCeEngine(connectionString);
            if (File.Exists(databaseDataPath) == false)
                engine.CreateDatabase();

            var syntaxProvider = new SqlCeSyntaxProvider();
            SqlSyntaxContext.SqlSyntaxProvider = syntaxProvider;

            _database = new UmbracoDatabase(connectionString, "System.Data.SqlServerCe.4.0", Mock.Of<ILogger>());

            // First remove anything in the database
            var creation = new DatabaseSchemaCreation(_database, Mock.Of<ILogger>(), syntaxProvider);
            creation.UninstallDatabaseSchema();

            // Then populate it with fresh data
            _database.CreateDatabaseSchema(false);

            _database.Execute("UPDATE umbracoUser SET userName = 'admin', userPassword = 'W477AMlLwwJQeAGlPZKiEILr8TA=', userEmail = 'none' WHERE id = 0"); // password: test

            // Recycle app pool so the new user can log in
            //var webConfigFilePath = string.Format(@"{0}\web.config", webUiPath);
            //File.SetLastWriteTime(webConfigFilePath, DateTime.Now);

            // Disable medium trust
            transform = TransformWebConfig("Release");

            Driver = new FirefoxDriver();
            BaseUrl = "http://localhost:61639/";
            _verificationErrors = new StringBuilder();
        }

        [TearDown]
        public virtual void TearDown()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }

            // Re-enable medium trust
            var transform = TransformWebConfig("Debug");

            Assert.AreEqual("", _verificationErrors.ToString());
        }

        private static string TransformWebConfig(string configurationToUse)
        {
            var assemblyPath = TestHelper.CurrentAssemblyDirectory;
            var toolsPath = Path.Combine(assemblyPath, @"..\..\..\..\tools\ConfigTransformTool\");
            var webUiPath = Path.GetFullPath(new Uri(Path.Combine(assemblyPath, @"..\..\..\Umbraco.Web.UI\")).LocalPath);

            var fileToTransform = webUiPath + "web.config";
            var transformFile = string.Format("{0}web.Template.{1}.config", webUiPath, configurationToUse);

            var psi = new ProcessStartInfo(string.Format("{0}ctt.exe", toolsPath), string.Format("s:\"{0}\" t:\"{1}\" d:\"{2}\" pw v", fileToTransform, transformFile, fileToTransform))
            {
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string[] result = {string.Empty};
            using (var process = new Process { StartInfo = psi })
            {
                // delegate for writing the process output to the response output
                Action<Object, DataReceivedEventArgs> dataReceived = ((sender, e) =>
                {
                    if (e.Data != null) // sometimes a random event is received with null data, not sure why - I prefer to leave it out
                    {
                        result[0] +=  e.Data;
                        result[0] += "\r\n";
                    }
                });

                process.OutputDataReceived += new DataReceivedEventHandler(dataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(dataReceived);

                // start the process and start reading the standard and error outputs
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                // wait for the process to exit
                process.WaitForExit();

                // an exit code other than 0 generally means an error
                if (process.ExitCode != 0)
                {
                    result[0] = result[0] + "\r\n - Exited with exitcode " + process.ExitCode;
                }
            }

            return result[0];
        }
    }
}
