using System;
using System.Collections.Generic;
using System.Reflection;
using umbraco.interfaces;

namespace Umbraco.Core.Resolving
{
	/// <summary>
	/// A resolver to return all IAction objects
	/// </summary>
	internal sealed class ActionsResolver : ManyObjectsResolverBase<ActionsResolver, IAction>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="packageActions"></param>		
		internal ActionsResolver(IEnumerable<Type> packageActions)
			: base(packageActions)
		{

		}

		/// <summary>
		/// Gets the <see cref="IPackageAction"/> implementations.
		/// </summary>
		public IEnumerable<IAction> Actions
		{
			get
			{
				return Values;
			}
		}

		protected override IEnumerable<IAction> CreateInstances()
		{
			var actions = new List<IAction>();
			var foundIActions = PluginManager.Current.ResolveActions();
			foreach (var type in foundIActions)
			{
				IAction typeInstance;
				var instance = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
				//if the singletone initializer is not found, try simply creating an instance of the IAction if it supports public constructors
				if (instance == null)
					typeInstance = PluginManager.Current.CreateInstance<IAction>(type);
				else
					typeInstance = instance.GetValue(null, null) as IAction;

				if (typeInstance != null)
				{
					actions.Add(typeInstance);
				}
			}
			return actions;
		}

	}
}