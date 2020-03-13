using Umbraco.Core.Configuration;

namespace Umbraco.Tests.Integration.Infrastructure
{
    public class TestGlobalSettings : IGlobalSettings
    {
        public string ReservedUrls => throw new System.NotImplementedException();

        public string ReservedPaths => throw new System.NotImplementedException();

        public string Path => throw new System.NotImplementedException();

        public string ConfigurationStatus { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int TimeOutInMinutes => throw new System.NotImplementedException();

        public string DefaultUILanguage => throw new System.NotImplementedException();

        public bool HideTopLevelNodeFromPath => throw new System.NotImplementedException();

        public bool UseHttps => throw new System.NotImplementedException();

        public int VersionCheckPeriod => throw new System.NotImplementedException();

        public string UmbracoPath => "/Umbraco";

        public string UmbracoCssPath => throw new System.NotImplementedException();

        public string UmbracoScriptsPath => throw new System.NotImplementedException();

        public string UmbracoMediaPath => throw new System.NotImplementedException();

        public bool IsSmtpServerConfigured => throw new System.NotImplementedException();

        public ISmtpSettings SmtpSettings => throw new System.NotImplementedException();

        public bool InstallMissingDatabase => throw new System.NotImplementedException();

        public bool InstallEmptyDatabase => throw new System.NotImplementedException();

        public bool DisableElectionForSingleServer => throw new System.NotImplementedException();

        public string RegisterType => throw new System.NotImplementedException();

        public string DatabaseFactoryServerVersion => throw new System.NotImplementedException();

        public string MainDomLock => throw new System.NotImplementedException();
    }
}
