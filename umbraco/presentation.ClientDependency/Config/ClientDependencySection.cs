using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Linq;

namespace umbraco.presentation.ClientDependency.Config
{
	public class ClientDependencySection : ConfigurationSection
	{
		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get { return (ProviderSettingsCollection)base["providers"]; }
		}

		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("defaultProvider", DefaultValue = "PageHeaderProvider")]
		public string DefaultProvider
		{
			get { return (string)base["defaultProvider"]; }
			set { base["defaultProvider"] = value; }
		}

		/// <summary>
		/// This is the folder that composite script/css files will be stored once they are combined.
		/// </summary>
		[StringValidator(MinLength = 1)]
		[ConfigurationProperty("compositeFilePath", DefaultValue = "~/App_Data/ClientDependency")]
		public string CompositeFilePath
		{
			get { return (string)base["compositeFilePath"]; }
			set { base["compositeFilePath"] = value; }
		}

        /// <summary>
        /// Flags whether or not to enable composite file script creation.
        /// Composite file creation will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        [ConfigurationProperty("enableCompositeFiles", DefaultValue = "true")]
        public bool EnableCompositeFiles
        {
            get { return (bool)base["enableCompositeFiles"]; }
            set { base["enableCompositeFiles"] = value; }
        }
        
        /// <summary>
        /// Set the default isDebugMode property for all loaders
        /// </summary>
        [ConfigurationProperty("isDebugMode", DefaultValue = "false")]
        public bool IsDebugMode
        {
            get { return (bool)base["isDebugMode"]; }
            set { base["isDebugMode"] = value; }
        }

		/// <summary>
		/// The configuration section to set the FileBasedDependencyExtensionList. This is a comma separated list.
		/// </summary>
		/// <remarks>
		/// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
		/// </remarks>
		[ConfigurationProperty("fileDependencyExtensions", DefaultValue = "js,css")]
		protected string FileBasedDepdendenyExtensions
		{
			get { return (string)base["fileDependencyExtensions"]; }
			set { base["fileDependencyExtensions"] = value; }
		}

		/// <summary>
		/// The file extensions of Client Dependencies that are file based as opposed to request based.
		/// Any file that doesn't have the extensions listed here will be request based, request based is
		/// more overhead for the server to process.
		/// </summary>
		/// <example>
		/// A request based JavaScript file may be  a .ashx that dynamically creates JavaScript server side.
		/// </example>
		/// <remarks>
		/// If this is not explicitly set, then the extensions 'js' and 'css' are the defaults.
		/// </remarks>
		public List<string> FileBasedDependencyExtensionList
		{
			get
			{
				return FileBasedDepdendenyExtensions.Split(',')
					.Select(x => x.Trim().ToLower())
					.ToList();
			}
		}
	}

}
