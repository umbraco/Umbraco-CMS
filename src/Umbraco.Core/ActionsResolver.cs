using System;
using System.Collections.Generic;
using System.Globalization;
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
	public sealed class ActionsResolver : LazyManyObjectsResolverBase<ActionsResolver, IAction>
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
        /// Gets the <see cref="IAction"/> implementations.
        /// </summary>
        public IEnumerable<IAction> Actions
		{
			get
			{
				return Values;
			}
		}

        /// <summary>
        /// This method will return a list of IAction's based on a string (letter) list. Each character in the list may represent
        /// an IAction. This will associate any found IActions based on the Letter property of the IAction with the character being referenced.
        /// </summary>
        /// <param name="actions"></param>
        /// <returns>returns a list of actions that have an associated letter found in the action string list</returns>
        public IEnumerable<IAction> FromActionSymbols(IEnumerable<string> actions)
	    {
	        var allActions = Actions.ToArray();
	        return actions
                .Select(c => allActions.FirstOrDefault(a => a.Letter.ToString(CultureInfo.InvariantCulture) == c))
                .WhereNotNull()
                .ToArray();
	    }

	    /// <summary>
	    /// Returns the string (letter) representation of the actions that make up the actions collection
	    /// </summary>
	    /// <returns></returns>
	    public IEnumerable<string> ToActionSymbols(IEnumerable<IAction> actions)
	    {
	        return actions.Select(x => x.Letter.ToString(CultureInfo.InvariantCulture)).ToArray();
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