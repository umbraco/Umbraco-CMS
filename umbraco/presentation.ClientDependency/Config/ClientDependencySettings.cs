using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using umbraco.presentation.ClientDependency.Providers;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Configuration;

namespace umbraco.presentation.ClientDependency.Config
{
	public class ClientDependencySettings
	{

		private ClientDependencySettings()
		{
			LoadProviders();
		}

		/// <summary>
		/// Singleton
		/// </summary>
		public static ClientDependencySettings Instance
		{
			get
			{					
				return m_Settings;
			}
		}

		private static readonly ClientDependencySettings m_Settings = new ClientDependencySettings();

		private object m_Lock = new object();
		private ClientDependencyProvider m_Provider = null;
		private ClientDependencyProviderCollection m_Providers = null;

        public const string ConfigurationSectionName = "clientDependency";

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
        public List<string> FileBasedDependencyExtensionList { get; set; }

        /// <summary>
        /// Flags whether or not to enable composite file script creation.
        /// Composite file creation will increase performance in the case of cache turnover or application
        /// startup since the files are already combined and compressed.
        /// This also allows for the ability to easily clear the cache so the files are refreshed.
        /// </summary>
        public bool EnableCompositeFiles { get; set; }

        public bool IsDebugMode { get; set; }

		public ClientDependencyProvider DefaultProvider
		{
			get
			{				
				return m_Provider;
			}
		}
		public ClientDependencyProviderCollection ProviderCollection
		{
			get
			{
				return m_Providers;
			}
		}
        public DirectoryInfo CompositeFilePath { get; set; }

		private void LoadProviders()
		{
			if (m_Provider == null)
			{
				lock (m_Lock)
				{
					// Do this again to make sure _provider is still null
					if (m_Provider == null)
					{
                        ClientDependencySection section = (ClientDependencySection)ConfigurationManager.GetSection("clientDependency");

						m_Providers = new ClientDependencyProviderCollection();

						// if there is no section found, then add the standard providers to the collection with the standard 
						// default provider
						if (section != null)
						{
							// Load registered providers and point _provider to the default provider	
							ProvidersHelper.InstantiateProviders(section.Providers, m_Providers, typeof(ClientDependencyProvider));							
						}
						else
						{
							//create a new section with the default settings
							section = new ClientDependencySection();							
							PageHeaderProvider php = new PageHeaderProvider();
							php.Initialize(PageHeaderProvider.DefaultName, null);
							m_Providers.Add(php);
							ClientSideRegistrationProvider csrp = new ClientSideRegistrationProvider();
							csrp.Initialize(ClientSideRegistrationProvider.DefaultName, null);
							m_Providers.Add(csrp);						
						}

						//set the default
						m_Provider = m_Providers[section.DefaultProvider];
						if (m_Provider == null)
							throw new ProviderException("Unable to load default ClientDependency provider");

                        IsDebugMode = section.IsDebugMode;
                        EnableCompositeFiles = section.EnableCompositeFiles;
                        FileBasedDependencyExtensionList = section.FileBasedDependencyExtensionList;
						CompositeFilePath = new DirectoryInfo(HttpContext.Current.Server.MapPath(section.CompositeFilePath));
					}
				}
			}
		}
	}
}
