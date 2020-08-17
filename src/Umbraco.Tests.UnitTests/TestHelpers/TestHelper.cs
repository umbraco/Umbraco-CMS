using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Net;
using Umbraco.Tests.Common;
using Umbraco.Web;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Routing;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Tests.UnitTests.TestHelpers
{
    public static class TestHelper
    {
        private static readonly TestHelperInternal _testHelperInternal = new TestHelperInternal();
        private class TestHelperInternal : TestHelperBase
        {
            public TestHelperInternal() : base(typeof(TestHelperInternal).Assembly)
            {

            }

            public override IBackOfficeInfo GetBackOfficeInfo()
            {
                throw new NotImplementedException();
            }

            public override IDbProviderFactoryCreator DbProviderFactoryCreator { get; }
            public override IBulkSqlInsertProvider BulkSqlInsertProvider { get; }
            public override IMarchal Marchal { get; }
            public override IHostingEnvironment GetHostingEnvironment()
                => new AspNetCoreHostingEnvironment(Mock.Of<IHostingSettings>(), new TestWebHostEnvironment());

            public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime()
            {
                throw new NotImplementedException();
            }

            public override IIpResolver GetIpResolver()
            {
                throw new NotImplementedException();
            }
        }

        public static ITypeFinder GetTypeFinder() => _testHelperInternal.GetTypeFinder();

        public static TypeLoader GetMockedTypeLoader() => _testHelperInternal.GetMockedTypeLoader();

        public static Configs GetConfigs() => _testHelperInternal.GetConfigs();

        public static IRuntimeState GetRuntimeState() => _testHelperInternal.GetRuntimeState();

        public static IBackOfficeInfo GetBackOfficeInfo() => _testHelperInternal.GetBackOfficeInfo();

        public static IConfigsFactory GetConfigsFactory() => _testHelperInternal.GetConfigsFactory();

        /// <summary>
        /// Gets the working directory of the test project.
        /// </summary>
        /// <value>The assembly directory.</value>
        public static string WorkingDirectory => _testHelperInternal.WorkingDirectory;

        public static IShortStringHelper ShortStringHelper => _testHelperInternal.ShortStringHelper;
        public static IJsonSerializer JsonSerializer => _testHelperInternal.JsonSerializer;
        public static IVariationContextAccessor VariationContextAccessor => _testHelperInternal.VariationContextAccessor;
        public static IDbProviderFactoryCreator DbProviderFactoryCreator => _testHelperInternal.DbProviderFactoryCreator;
        public static IBulkSqlInsertProvider BulkSqlInsertProvider => _testHelperInternal.BulkSqlInsertProvider;
        public static IMarchal Marchal => _testHelperInternal.Marchal;
        public static ICoreDebugSettings CoreDebugSettings => _testHelperInternal.CoreDebugSettings;


        public static IIOHelper IOHelper => _testHelperInternal.IOHelper;
        public static IMainDom MainDom => _testHelperInternal.MainDom;
        public static UriUtility UriUtility => _testHelperInternal.UriUtility;

        public static IWebRoutingSettings WebRoutingSettings => _testHelperInternal.WebRoutingSettings;


        /// <summary>
        /// Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based on a virtual path name
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string MapPathForTestFiles(string relativePath) => _testHelperInternal.MapPathForTestFiles(relativePath);

        

        public static void CreateDirectories(string[] directories)
        {
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                if (directoryInfo.Exists == false)
                    Directory.CreateDirectory(IOHelper.MapPath(directory));
            }
        }

        public static void CleanDirectories(string[] directories)
        {
            var preserves = new Dictionary<string, string[]>
            {
                { Constants.SystemDirectories.MvcViews, new[] {"dummy.txt"} }
            };
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
                if (directoryInfo.Exists)
                    foreach (var x in directoryInfo.GetFiles().Where(x => preserve == null || preserve.Contains(x.Name) == false))
                        x.Delete();
            }
        }

        public static void CleanUmbracoSettingsConfig()
        {
            var currDir = new DirectoryInfo(WorkingDirectory);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile))
                File.Delete(umbracoSettingsFile);
        }

      

        public static void DeleteDirectory(string path)
        {
            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    File.Delete(file);
            });

            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                Directory.Delete(path, true);
            });
        }

        public static void TryAssert(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<AssertionException>(action, maxTries, waitMilliseconds);
        }

        public static void Try(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<Exception>(action, maxTries, waitMilliseconds);
        }

        public static void Try<T>(Action action, int maxTries = 5, int waitMilliseconds = 200)
            where T : Exception
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (T)
                {
                    if (tries++ > maxTries)
                        throw;
                    Thread.Sleep(waitMilliseconds);
                }
            }
        }

        public static IUmbracoVersion GetUmbracoVersion() => _testHelperInternal.GetUmbracoVersion();

        public static IRegister GetRegister() => _testHelperInternal.GetRegister();

        public static IHostingEnvironment GetHostingEnvironment() => _testHelperInternal.GetHostingEnvironment();

        public static ILoggingConfiguration GetLoggingConfiguration(IHostingEnvironment hostingEnv) => _testHelperInternal.GetLoggingConfiguration(hostingEnv);

        public static IApplicationShutdownRegistry GetHostingEnvironmentLifetime() => _testHelperInternal.GetHostingEnvironmentLifetime();

        public static IIpResolver GetIpResolver() => _testHelperInternal.GetIpResolver();

        public static IRequestCache GetRequestCache() => _testHelperInternal.GetRequestCache();

       

        public static IPublishedUrlProvider GetPublishedUrlProvider() => _testHelperInternal.GetPublishedUrlProvider();
    }

    internal class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment()
        {
            EnvironmentName = "UnitTest";
            ApplicationName = "UnitTest";
            ContentRootPath = "/";
            WebRootPath = "/wwwroot";
        }

        public string EnvironmentName { get ; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
    }
}
