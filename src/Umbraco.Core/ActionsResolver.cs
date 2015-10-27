using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IAction objects
	/// </summary>
	internal sealed class ActionsResolver : LazyManyObjectsResolverBase<ActionsResolver, IAction>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="packageActions"></param>		
	    internal ActionsResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> packageActions)
            : base(serviceProvider, logger, packageActions)
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

        /// <summary>
        /// Gets an Action if it exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal IAction GetAction<T>()
            where T : IAction
        {
            return Actions.SingleOrDefault(x => x.GetType() == typeof (T));
        }

		protected override IEnumerable<IAction> CreateInstances()
		{					
			var actions = new List<IAction>();
			var foundIActions = InstanceTypes;
			foreach (var type in foundIActions)
			{
				IAction typeInstance;
				var instance = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
				//if the singletone initializer is not found, try simply creating an instance of the IAction if it supports public constructors
				if (instance == null)
					typeInstance = ServiceProvider.GetService(type) as IAction;
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