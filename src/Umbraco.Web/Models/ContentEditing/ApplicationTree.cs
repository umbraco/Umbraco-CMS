using System;
using System.Diagnostics;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.ContentEditing
{
    [DebuggerDisplay("Tree - {Alias} ({ApplicationAlias})")]
    public class ApplicationTree
    {
        //private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>();

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        ///// </summary>
        //public ApplicationTree() { }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        ///// </summary>
        ///// <param name="initialize">if set to <c>true</c> [initialize].</param>
        ///// <param name="sortOrder">The sort order.</param>
        ///// <param name="applicationAlias">The application alias.</param>
        ///// <param name="alias">The tree alias.</param>
        ///// <param name="title">The tree title.</param>
        ///// <param name="iconClosed">The icon closed.</param>
        ///// <param name="iconOpened">The icon opened.</param>
        ///// <param name="type">The tree type.</param>
        //public ApplicationTree(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        //{
        //    //Initialize = initialize;
        //    SortOrder = sortOrder;
        //    ApplicationAlias = applicationAlias;
        //    Alias = alias;
        //    Title = title;
        //    IconClosed = iconClosed;
        //    IconOpened = iconOpened;
        //    Type = type;

        //}

        public ApplicationTree(int sortOrder, string applicationAlias, string alias, string title)
        {
            SortOrder = sortOrder;
            ApplicationAlias = applicationAlias;
            Alias = alias;
            Title = title;
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> should initialize.
        ///// </summary>
        ///// <value><c>true</c> if initialize; otherwise, <c>false</c>.</value>
        //public bool Initialize { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets the application alias.
        /// </summary>
        /// <value>The application alias.</value>
        public string ApplicationAlias { get; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; }

        /// <summary>
        /// Gets or sets the tree title (fallback if the tree alias isn't localized)
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        ///// <summary>
        ///// Gets or sets the icon closed.
        ///// </summary>
        ///// <value>The icon closed.</value>
        //public string IconClosed { get; set; }

        ///// <summary>
        ///// Gets or sets the icon opened.
        ///// </summary>
        ///// <value>The icon opened.</value>
        //public string IconOpened { get; set; }

        ///// <summary>
        ///// Gets or sets the tree type assembly name.
        ///// </summary>
        ///// <value>The type.</value>
        //public string Type { get; set; }

        /// <summary>
        /// Returns the localized root node display name
        /// </summary>
        /// <param name="textService"></param>
        /// <returns></returns>
        public string GetRootNodeDisplayName(ILocalizedTextService textService)
        {
            var label = $"[{Alias}]";

            // try to look up a the localized tree header matching the tree alias
            var localizedLabel = textService.Localize("treeHeaders/" + Alias);

            // if the localizedLabel returns [alias] then return the title if it's defined
            if (localizedLabel != null && localizedLabel.Equals(label, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(Title) == false)
                    label = Title;
            }
            else
            {
                // the localizedLabel translated into something that's not just [alias], so use the translation
                label = localizedLabel;
            }

            return label;
        }

        //private Type _runtimeType;

        ///// <summary>
        ///// Returns the CLR type based on it's assembly name stored in the config
        ///// </summary>
        ///// <returns></returns>
        //public Type GetRuntimeType()
        //{
        //    return _runtimeType ?? (_runtimeType = System.Type.GetType(Type));
        //}

        ///// <summary>
        ///// Used to try to get and cache the tree type
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //internal static Type TryGetType(string type)
        //{
        //    try
        //    {
        //        return ResolvedTypes.GetOrAdd(type, s =>
        //        {
        //            var result = System.Type.GetType(type);
        //            if (result != null)
        //            {
        //                return result;
        //            }

        //            //we need to implement a bit of a hack here due to some trees being renamed and backwards compat
        //            var parts = type.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //            if (parts.Length != 2)
        //                throw new InvalidOperationException("Could not resolve type");
        //            if (parts[1].Trim() != "Umbraco.Web" || parts[0].StartsWith("Umbraco.Web.Trees") == false || parts[0].EndsWith("Controller"))
        //                throw new InvalidOperationException("Could not resolve type");

        //            //if it's one of our controllers but it's not suffixed with "Controller" then add it and try again
        //            var tempType = parts[0] + "Controller, Umbraco.Web";

        //            result = System.Type.GetType(tempType);
        //            if (result != null)
        //                return result;

        //            throw new InvalidOperationException("Could not resolve type");
        //        });
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        //swallow, this is our own exception, couldn't find the type
        //        // fixme bad use of exceptions here!
        //        return null;
        //    }
        //}
    }
}
