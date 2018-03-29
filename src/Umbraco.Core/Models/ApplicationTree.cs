using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Umbraco.Core.Models
{
    [DebuggerDisplay("Tree - {Title} ({ApplicationAlias})")]
    public class ApplicationTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        public ApplicationTree() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="alias">The tree alias.</param>
        /// <param name="title">The tree title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpened">The icon opened.</param>
        /// <param name="type">The tree type.</param>
        public ApplicationTree(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        {
            this.Initialize = initialize;
            this.SortOrder = sortOrder;
            this.ApplicationAlias = applicationAlias;
            this.Alias = alias;
            this.Title = title;
            this.IconClosed = iconClosed;
            this.IconOpened = iconOpened;
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> should initialize.
        /// </summary>
        /// <value><c>true</c> if initialize; otherwise, <c>false</c>.</value>
        public bool Initialize { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets the application alias.
        /// </summary>
        /// <value>The application alias.</value>
        public string ApplicationAlias { get; private set; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets the tree title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the icon closed.
        /// </summary>
        /// <value>The icon closed.</value>
        public string IconClosed { get; set; }

        /// <summary>
        /// Gets or sets the icon opened.
        /// </summary>
        /// <value>The icon opened.</value>
        public string IconOpened { get; set; }

        /// <summary>
        /// Gets or sets the tree type assembly name.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        private Type _runtimeType;
        
        /// <summary>
        /// Returns the CLR type based on it's assembly name stored in the config
        /// </summary>
        /// <returns></returns>
        public Type GetRuntimeType()
        {
            if (_runtimeType != null)
                return _runtimeType;

            _runtimeType = TryGetType(Type);
            return _runtimeType;
        }

        /// <summary>
        /// Used to try to get and cache the tree type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type TryGetType(string type)
        {
            try
            {
                return ResolvedTypes.GetOrAdd(type, s =>
                {
                    var result = System.Type.GetType(type);
                    if (result != null)
                    {
                        return result;
                    }

                    //we need to implement a bit of a hack here due to some trees being renamed and backwards compat
                    var parts = type.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        if (parts[1].Trim() == "umbraco" && parts[0].StartsWith("Umbraco.Web.Trees") && parts[0].EndsWith("Controller") == false)
                        {
                            //if it's one of our controllers but it's not suffixed with "Controller" then add it and try again
                            var tempType = parts[0] + "Controller, umbraco";

                            result = System.Type.GetType(tempType);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }

                    throw new InvalidOperationException("Could not resolve type");
                });
            }
            catch (InvalidOperationException)
            {
                //swallow, this is our own exception, couldn't find the type
                return null;
            }
        }

        private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>();
    }
}