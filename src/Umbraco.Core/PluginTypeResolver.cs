using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Umbraco.Core
{

	/// <summary>
	/// Used to resolve all plugin types and cache them
	/// </summary>
	/// <remarks>
	/// 
	/// This class should be used to resolve all plugin types, the TypeFinder should not be used directly!
	/// 
	/// This class can expose extension methods to resolve custom plugins
	/// 
	/// </remarks>
	internal class PluginTypeResolver
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
		private IEnumerable<Assembly> _assemblies;

		/// <summary>
		/// Gets/sets which assemblies to scan when type finding, generally used for unit testing, if not explicitly set
		/// this will search all assemblies known to have plugins and exclude ones known to not have them.
		/// </summary>
		internal IEnumerable<Assembly> AssembliesToScan
		{
			get { return _assemblies ?? (_assemblies = TypeFinder2.GetAssembliesWithKnownExclusions()); }
			set { _assemblies = value; }
		}

		/// <summary>
		/// Used to resolve and create instances of the specified type based on the resolved/cached plugin types
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="throwException">set to true if an exception is to be thrown if there is an error during instantiation</param>
		/// <returns></returns>
		internal IEnumerable<T> FindAndCreateInstances<T>(bool throwException = false)
		{
			var types = ResolveTypes<T>();
			return CreateInstances<T>(types, throwException);
		}

		/// <summary>
		/// Used to create instances of the specified type based on the resolved/cached plugin types
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="types"></param>
		/// <param name="throwException">set to true if an exception is to be thrown if there is an error during instantiation</param>
		/// <returns></returns>
		internal IEnumerable<T> CreateInstances<T>(IEnumerable<Type> types, bool throwException = false)
		{
			var instances = new List<T>();
			foreach (var t in types)
			{
				try
				{
					var typeInstance = (T)Activator.CreateInstance(t);
					instances.Add(typeInstance);
				}
				catch (Exception ex)
				{
					//TODO: Need to fix logging so this doesn't bork if no SQL connection
					//Log.Add(LogTypes.Error, -1, "Error loading ILookup: " + ex.ToString());
					if (throwException)
					{
						throw ex;
					}
				}
			}
			return instances;
		}

		/// <summary>
		/// Used to create an instance of the specified type based on the resolved/cached plugin types
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="throwException"></param>
		/// <returns></returns>
		internal T CreateInstance<T>(Type type, bool throwException = false)
		{
			var instances = CreateInstances<T>(new[] {type}, throwException);
			return instances.FirstOrDefault();
		}

		private IEnumerable<Type> ResolveTypes<T>(Func<IEnumerable<Type>> finder)
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

					foreach (var t in finder())
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
		/// Generic method to find the specified type and cache the result
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal IEnumerable<Type> ResolveTypes<T>()
		{
			return ResolveTypes<T>(() => TypeFinder.FindClassesOfType<T>(AssembliesToScan));
		}

		/// <summary>
		/// Generic method to find the specified type that has an attribute and cache the result
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TAttribute"></typeparam>
		/// <returns></returns>
		internal IEnumerable<Type> ResolveTypesWithAttribute<T, TAttribute>()
			where TAttribute : Attribute
		{
			return ResolveTypes<T>(() => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(AssembliesToScan));
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
