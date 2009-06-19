using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using umbraco.presentation.ClientDependency.Providers;
using System.Web.Configuration;
using System.Configuration.Provider;

namespace umbraco.presentation.ClientDependency
{

	public class ClientDependencyHelper
	{

		private static ClientDependencyProvider m_Provider = null;
		private static ClientDependencyProviderCollection m_Providers = null;
		private static object m_Lock = new object();

		public static ClientDependencyProvider DefaultProvider
		{
            get
            {
                LoadProviders();
                return m_Provider;
            }
		}
        public static ClientDependencyProviderCollection ProviderCollection
		{
            get
            {
                LoadProviders();
                return m_Providers;
            }
		}

		private static void LoadProviders()
		{			
			if (m_Provider == null)
			{
				lock (m_Lock)
				{
					// Do this again to make sure _provider is still null
					if (m_Provider == null)
					{
						ClientDependencySection section = (ClientDependencySection)WebConfigurationManager.GetSection("system.web/imageService");
						
						m_Providers = new ClientDependencyProviderCollection();

						// if there is no section found, then add the standard providers to the collection with the standard 
						// default provider
						if (section != null)
						{
							// Load registered providers and point _provider to the default provider	
							ProvidersHelper.InstantiateProviders(section.Providers, m_Providers, typeof(ClientDependencyProvider));
							m_Provider = m_Providers[section.DefaultProvider];
							if (m_Provider == null)
								throw new ProviderException("Unable to load default ImageProvider");
						}
						else
						{
							PageHeaderProvider php = new PageHeaderProvider();
							php.Initialize(PageHeaderProvider.DefaultName, null);
							m_Providers.Add(php);
							ClientSideRegistrationProvider csrp = new ClientSideRegistrationProvider();
							csrp.Initialize(ClientSideRegistrationProvider.DefaultName, null);
							m_Providers.Add(csrp);
							m_Provider = m_Providers[PageHeaderProvider.DefaultName];
						}						
					}
				}
			}
		}

		/// <summary>
		/// Registers dependencies with the default provider
		/// </summary>
		/// <param name="control"></param>
		/// <param name="paths"></param>
		public static void RegisterClientDependencies(Control control, ClientDependencyPathCollection paths)
		{
			LoadProviders();
			ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, m_Provider);
			service.ProcessDependencies();
		}

		/// <summary>
		/// Registers dependencies with the provider name specified
		/// </summary>
		/// <param name="providerName"></param>
		/// <param name="control"></param>
		/// <param name="paths"></param>
		public static void RegisterClientDependencies(string providerName, Control control, ClientDependencyPathCollection paths)
		{
			LoadProviders();
			ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, m_Providers[providerName]);
			service.ProcessDependencies();
		}

		/// <summary>
		/// Registers dependencies with the provider specified by T
		/// </summary>
		public static void RegisterClientDependencies<T>(Control control, ClientDependencyPathCollection paths)
			where T: ClientDependencyProvider
		{
			LoadProviders();
			//need to find the provider with the type
			ClientDependencyProvider found = null;
			foreach (ClientDependencyProvider p in m_Providers)
			{
				if (p.GetType().Equals(typeof(T)))
				{
					found = p;
					break;
				}					
			}
			if (found == null)
				throw new ArgumentException("Could not find the ClientDependencyProvider specified by T");
			ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, found);
			service.ProcessDependencies();
		}		

        public static void RegisterClientDependencies(ClientDependencyProvider provider, Control control, ClientDependencyPathCollection paths)
        {
            LoadProviders();
            ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, provider);
			service.ProcessDependencies();
        }
	
		internal class ClientDependencyRegistrationService
		{
			public ClientDependencyRegistrationService(Control control, ClientDependencyPathCollection paths, ClientDependencyProvider provider)
			{
				m_RenderingControl = control;
				m_Paths = paths;
				m_Dependencies = FindDependencies(m_RenderingControl);
				m_Provider = provider;
			}
			
			private Control m_RenderingControl;
            private List<IClientDependencyFile> m_Dependencies = new List<IClientDependencyFile>();
			private ClientDependencyPathCollection m_Paths;
			private ClientDependencyProvider m_Provider;

			/// <summary>
			/// Recursively find all dependencies of this control and it's entire child control heirarchy.
			/// </summary>
			/// <param name="control"></param>
			/// <returns></returns>
            private List<IClientDependencyFile> FindDependencies(Control control)
			{
				// find dependencies
				Type controlType = control.GetType();
				List<IClientDependencyFile> dependencies = new List<IClientDependencyFile>();
				foreach (Attribute attribute in Attribute.GetCustomAttributes(controlType))
				{
					if (attribute is ClientDependencyAttribute)
					{
						dependencies.Add((ClientDependencyAttribute)attribute);
					}
				}

				// add child dependencies
				foreach (Control child in control.Controls)
				{
					if (child.GetType().Equals(typeof(ClientDependencyInclude)))
					{
						ClientDependencyInclude include = (ClientDependencyInclude)child;
						//dependencies.Add(new ClientDependencyAttribute(include.Priority, include.DependencyType, include.FilePath, include.PathName, include.InvokeJavascriptMethodOnLoad, include.CompositeGroupName));
                        dependencies.Add(include);
					}
					else if (child.HasControls())
					{
                        dependencies.AddRange(FindDependencies(child));
					}					
				}

				return dependencies;
			}

			public void ProcessDependencies()
			{
				m_Provider.RegisterDependencies(m_RenderingControl, m_Dependencies, m_Paths);
			}
		}
		
	}
}
