using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.presentation.ClientDependency
{

	/// <summary>
	/// Wraps a HashSet object for the client dependency file type and declares a equality operator
	/// </summary>
	public class ClientDependencyCollection : HashSet<IClientDependencyFile>
	{

		public ClientDependencyCollection() : base(new ClientDependencyComparer()) { }

		internal class ClientDependencyComparer : IEqualityComparer<IClientDependencyFile>
		{			
			#region IEqualityComparer<IClientDependencyFile> Members

			/// <summary>
			/// If the lowercased combination of the file path, dependency type and path name aliases are the same, 
			/// then they are the same dependency.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public bool Equals(IClientDependencyFile x, IClientDependencyFile y)
			{
				return (x.FilePath.ToLower().Trim() + x.DependencyType.ToString().ToLower() + x.PathNameAlias.ToLower().Trim() ==
					y.FilePath.ToLower().Trim() + y.DependencyType.ToString().ToLower() + y.PathNameAlias.ToLower().Trim());
			}

			public int GetHashCode(IClientDependencyFile obj)
			{
				return (obj.FilePath.ToLower().Trim() + obj.DependencyType.ToString().ToLower() + obj.PathNameAlias.ToLower().Trim())
					.GetHashCode();
			}

			#endregion
		}

	}

	
}
