using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Umbraco.Core
{

	/// <summary>
	/// Used to resolve all plugin types
	/// </summary>
	/// <remarks>
	/// 
	/// This class should be used to resolve all plugin types, the TypeFinder should not be used directly!
	/// 
	/// This class can expose extension methods to resolve custom plugins
	/// 
	/// </remarks>
	public class PluginTypeResolver
	{

		private PluginTypeResolver()
		{
		}

		static PluginTypeResolver _resolver;
		static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		/// <summary>
		/// We will ensure that no matter what, only one of these is created, this is to ensure that caching always takes place
		/// </summary>
		internal static PluginTypeResolver Current
		{
			get
			{
				if (_resolver == null)
				{
					using (new WriteLock(Lock))
					{
						_resolver = new PluginTypeResolver();
					}					
				}
				return _resolver;
			}
		}

		internal readonly TypeFinder2 TypeFinder = new TypeFinder2();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private readonly HashSet<TypeList> _types = new HashSet<TypeList>();

		/// <summary>
		/// Generic method to find the specified type and cache the result
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal IEnumerable<Type> ResolveTypes<T>()
		{
			using (var readLock = new UpgradeableReadLock(_lock))
			{
				//check if the TypeList already exists, if so return it, if not we'll create it
				var typeList = _types.SingleOrDefault(x => x.GetListType().IsType<T>());
				if (typeList == null)
				{
					//upgrade to a write lock since we're adding to the collection
					readLock.UpgradeToWriteLock();

					typeList = new TypeList<T>();

					foreach (var t in TypeFinder.FindClassesOfType<T>())
					{
						typeList.AddType(t);
					}

					//add the type list to the collection
					_types.Add(typeList);
				}
				return typeList.GetTypes();
			}


		}

		/// <summary>
		/// Used for unit tests
		/// </summary>
		/// <returns></returns>
		internal HashSet<TypeList> GetTypeLists()
		{
			return _types;
		}

		#region Private classes
		internal abstract class TypeList
		{
			public abstract void AddType(Type t);
			public abstract Type GetListType();
			public abstract IEnumerable<Type> GetTypes();
		}

		internal class TypeList<T> : TypeList
		{
			private readonly List<Type> _types = new List<Type>();

			public override void AddType(Type t)
			{
				if (t.IsType<T>())
				{
					_types.Add(t);
				}
			}

			public override Type GetListType()
			{
				return typeof(T);
			}

			public override IEnumerable<Type> GetTypes()
			{
				return _types;
			}
		}
		#endregion

	}
}
