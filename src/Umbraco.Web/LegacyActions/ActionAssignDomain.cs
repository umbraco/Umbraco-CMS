using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using umbraco.interfaces;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web.LegacyActions
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
            return Actions.SingleOrDefault(x => x.GetType() == typeof(T));
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

    public interface IAction
    {
        char Letter { get; }
        bool ShowInNotifier { get; }
        bool CanBePermissionAssigned { get; }
        string Icon { get; }
        string Alias { get; }
        string JsFunctionName { get; }
        /// <summary>
        /// A path to a supporting JavaScript file for the IAction. A script tag will be rendered out with the reference to the JavaScript file.
        /// </summary>
		string JsSource { get; }
    }

    /// <summary>
    /// This action is invoked when a domain is being assigned to a document
    /// </summary>
    public class ActionAssignDomain : IAction
	{
		//create singleton
#pragma warning disable 612,618
		private static readonly ActionAssignDomain m_instance = new ActionAssignDomain();
#pragma warning restore 612,618

		/// <summary>
		/// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
		/// All Umbraco assemblies should use the singleton instantiation (this.Instance)
		/// When this applicatio is refactored, this constuctor should be made private.
		/// </summary>
		[Obsolete("Use the singleton instantiation instead of a constructor")]
		public ActionAssignDomain() { }

		public static ActionAssignDomain Instance
		{
			get { return m_instance; }
		}

		#region IAction Members

		public char Letter
		{
			get
			{
				return 'I';
			}
		}

		public string JsFunctionName
		{
			get
			{
				return string.Format("{0}.actionAssignDomain()", ClientTools.Scripts.GetAppActions);
			}
		}

		public string JsSource
		{
			get
			{
				return null;
			}
		}

		public string Alias
		{
			get
			{
				return "assignDomain";
			}
		}

		public string Icon
		{
			get
			{
                return "home";
			}
		}

		public bool ShowInNotifier
		{
			get
			{
				return false;
			}
		}
		public bool CanBePermissionAssigned
		{
			get
			{
				return true;
			}
		}
		#endregion
	}
}
