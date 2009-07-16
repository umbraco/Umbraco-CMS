using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;
using umbraco.presentation.ClientDependency.Providers;
using System.Web.Configuration;
using System.Configuration.Provider;
using umbraco.presentation.ClientDependency.Config;
using umbraco.presentation.ClientDependency.Controls;

namespace umbraco.presentation.ClientDependency
{

	public class ClientDependencyHelper
	{
		

		/// <summary>
		/// Registers dependencies with the default provider
		/// </summary>
		/// <param name="control"></param>
		/// <param name="paths"></param>
		public static void RegisterClientDependencies(Control control, ClientDependencyPathCollection paths)
		{
			ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, 
				ClientDependencySettings.Instance.DefaultProvider);
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
			ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths,
				ClientDependencySettings.Instance.ProviderCollection[providerName]);
			service.ProcessDependencies();
		}

		/// <summary>
		/// Registers dependencies with the provider specified by T
		/// </summary>
		public static void RegisterClientDependencies<T>(Control control, ClientDependencyPathCollection paths)
			where T: ClientDependencyProvider
		{
			//need to find the provider with the type
			ClientDependencyProvider found = null;
			foreach (ClientDependencyProvider p in ClientDependencySettings.Instance.ProviderCollection)
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
            ClientDependencyRegistrationService service = new ClientDependencyRegistrationService(control, paths, provider);
			service.ProcessDependencies();
        }
	
		/// <summary>
		/// This class is used to find all dependencies for the current request, d
		/// </summary>
		internal class ClientDependencyRegistrationService
		{
			public ClientDependencyRegistrationService(Control control, ClientDependencyPathCollection paths, ClientDependencyProvider provider)
			{
				m_RenderingControl = control;
				m_Paths = paths;
				//find all dependencies
				m_Dependencies = FindDependencies(m_RenderingControl);
				m_Provider = provider;
			}
			
			private Control m_RenderingControl;
			private ClientDependencyList m_Dependencies = new ClientDependencyList();
			private ClientDependencyPathCollection m_Paths;
			private ClientDependencyProvider m_Provider;

			/// <summary>
			/// Recursively find all dependencies of this control and it's entire child control heirarchy.
			/// </summary>
			/// <param name="control"></param>
			/// <returns></returns>
			private ClientDependencyList FindDependencies(Control control)
			{
				// find dependencies
				Type controlType = control.GetType();
				ClientDependencyList dependencies = new ClientDependencyList();
				foreach (Attribute attribute in Attribute.GetCustomAttributes(controlType))
				{
					if (attribute is ClientDependencyAttribute)
					{
						dependencies.Add((ClientDependencyAttribute)attribute);
					}
				}

				// add child dependencies
				Type iClientDependency = typeof(IClientDependencyFile);
				foreach (Control child in control.Controls)
				{
					if (iClientDependency.IsAssignableFrom(child.GetType()))
					{
						IClientDependencyFile include = (IClientDependencyFile)child;
						dependencies.Add(include);
					}				
					else
					{
						//recurse and de-duplicate!
						dependencies.UnionWith(FindDependencies(child));
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
