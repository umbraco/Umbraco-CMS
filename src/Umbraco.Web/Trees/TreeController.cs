using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// The base controller for all tree requests
    /// </summary>
    public abstract class TreeController : TreeControllerBase
    {
        private TreeAttribute _attribute;

        protected TreeController()
        {
            Initialize();
        }

        protected TreeController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
            Initialize();
        }

        protected TreeController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper) : base(umbracoContext, umbracoHelper)
        {
            Initialize();
        }        

        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public override string RootNodeDisplayName
        {
            get
            {

                //if title is defined, return that
                if(string.IsNullOrEmpty(_attribute.Title) == false)
                    return _attribute.Title;


                //try to look up a tree header matching the tree alias
                var localizedLabel = Services.TextService.Localize("treeHeaders/" + _attribute.Alias);
                if (string.IsNullOrEmpty(localizedLabel) == false)
                    return localizedLabel;

                //is returned to signal that a label was not found
                return "[" + _attribute.Alias + "]";
            }
        }

        /// <summary>
        /// Gets the current tree alias from the attribute assigned to it.
        /// </summary>
        public override string TreeAlias
        {
            get { return _attribute.Alias; }
        }

        private void Initialize()
        {
            //Locate the tree attribute
            var treeAttributes = GetType()
                .GetCustomAttributes(typeof(TreeAttribute), false)
                .OfType<TreeAttribute>()
                .ToArray();

            if (treeAttributes.Any() == false)
            {
                throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            _attribute = treeAttributes.First();
        }
    }
}
