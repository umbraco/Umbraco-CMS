using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web;

namespace umbraco.presentation.ClientDependency.Providers
{
	public abstract class ClientDependencyProvider : ProviderBase
	{
		protected Control DependantControl { get; private set; }
		protected ClientDependencyPathCollection FolderPaths { get; private set; }
		protected List<ClientDependencyAttribute> AllDependencies { get; private set; }

		protected abstract void RegisterJsFiles(List<IClientDependencyFile> jsDependencies);
		protected abstract void RegisterCssFiles(List<IClientDependencyFile> cssDependencies);

		public void RegisterDependencies(Control dependantControl, List<ClientDependencyAttribute> dependencies, ClientDependencyPathCollection paths)
		{
			DependantControl = dependantControl;
			AllDependencies = dependencies;
			FolderPaths = paths;

			UpdateFilePaths();

			//seperate the types into 2 lists for all dependencies without composite groups
			List<ClientDependencyAttribute> jsDependencies = AllDependencies.FindAll(
				delegate(ClientDependencyAttribute a)
				{
					return a.DependencyType == ClientDependencyType.Javascript && string.IsNullOrEmpty(a.CompositeGroupName);
				}
			);
			List<ClientDependencyAttribute> cssDependencies = AllDependencies.FindAll(
				delegate(ClientDependencyAttribute a)
				{
					return a.DependencyType == ClientDependencyType.Css && string.IsNullOrEmpty(a.CompositeGroupName);
				}
			);

			// sort by priority
			jsDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));
			cssDependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			RegisterCssFiles(cssDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));
			RegisterCssFiles(ProcessCompositeGroups(ClientDependencyType.Css));
			RegisterJsFiles(jsDependencies.ConvertAll<IClientDependencyFile>(a => { return (IClientDependencyFile)a; }));
			RegisterJsFiles(ProcessCompositeGroups(ClientDependencyType.Javascript));
		}

		private List<IClientDependencyFile> ProcessCompositeGroups(ClientDependencyType type)
		{
			List<ClientDependencyAttribute> dependencies = AllDependencies.FindAll(
				delegate(ClientDependencyAttribute a)
				{
					return a.DependencyType == type && !string.IsNullOrEmpty(a.CompositeGroupName);
				}
			);			
			List<string> groups = new List<string>();
			List<IClientDependencyFile> files = new List<IClientDependencyFile>();
			foreach (ClientDependencyAttribute a in dependencies)
			{
				if (!groups.Contains(a.CompositeGroupName))
				{
					string url = ProcessCompositeGroup(dependencies, a.CompositeGroupName, type);
					groups.Add(a.CompositeGroupName);
					files.Add(new SimpleDependencyFile(url, type));
					DependantControl.Page.Trace.Write("ClientDependency", "Composite group " + a.CompositeGroupName + " URL: " + url);
				}
			}
			return files;
		}

		/// <summary>
		/// Returns a full url with the encoded query strings for the handler which will process the composite group.
		/// </summary>
		/// <param name="dependencies"></param>
		/// <param name="groupName"></param>
		/// <returns></returns>
		private string ProcessCompositeGroup(List<ClientDependencyAttribute> dependencies, string groupName, ClientDependencyType type)
		{
			string handler = "{0}?s={1}&t={2}";
			StringBuilder files = new StringBuilder();
			List<ClientDependencyAttribute> byGroup = AllDependencies.FindAll(
				delegate(ClientDependencyAttribute a)
				{
					return a.CompositeGroupName == groupName;
				}
			);
			foreach (ClientDependencyAttribute a in dependencies)
			{
				files.Append(a.FilePath + ";");
			}
			string url = string.Format(handler, CompositeDependencyHandler.HandlerFileName, HttpContext.Current.Server.UrlEncode(EncodeTo64(files.ToString())), type.ToString());
			if (url.Length > CompositeDependencyHandler.MaxHandlerUrlLength)
				throw new ArgumentOutOfRangeException("The number of files in the composite group " + groupName + " creates a url handler address that exceeds the CompositeDependencyHandler MaxHandlerUrlLength. Reducing the amount of files in this composite group should fix the issue");
			return url;
		}

		private string EncodeTo64(string toEncode)
		{
			byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
			string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
			return returnValue;
		}

		/// <summary>
		/// Ensures the correctly resolved file path is set for each dependency (i.e. so that ~ are taken care of) and also
		/// prefixes the file path with the correct base path specified for the PathNameAlias if specified.
		/// </summary>
		/// <param name="dependencies"></param>
		/// <param name="paths"></param>
		/// <param name="control"></param>
		private void UpdateFilePaths()
		{
			foreach (ClientDependencyAttribute dependency in AllDependencies)
			{
				if (!string.IsNullOrEmpty(dependency.PathNameAlias))
				{
					ClientDependencyPath path = FolderPaths.Find(
						delegate(ClientDependencyPath p)
						{
							return p.Name == dependency.PathNameAlias;
						}
					);
					if (path == null)
					{
						throw new NullReferenceException("The PathNameAlias specified for dependency " + dependency.FilePath + " does not exist in the ClientDependencyPathCollection");
					}
					string basePath = path.ResolvedPath.EndsWith("/") ? path.ResolvedPath : path.ResolvedPath + "/";
					dependency.FilePath = basePath + dependency.FilePath;
				}
				else
				{
					dependency.FilePath = DependantControl.ResolveUrl(dependency.FilePath);
				}
			}
		}

		internal class SimpleDependencyFile : IClientDependencyFile
		{
			public SimpleDependencyFile(string filePath, ClientDependencyType type)
			{
				FilePath = filePath;
				DependencyType = type;
			}

			public string FilePath{get;set;}
			public ClientDependencyType DependencyType { get; set; }
			public string InvokeJavascriptMethodOnLoad { get; set; }
			public int Priority { get; set; }
		}

	}
}
