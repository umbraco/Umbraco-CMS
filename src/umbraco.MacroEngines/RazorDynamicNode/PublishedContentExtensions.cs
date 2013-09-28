using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web;
using umbraco.NodeFactory;
using umbraco.interfaces;
using Umbraco.Web.umbraco.presentation;
using Property = umbraco.NodeFactory.Property;

namespace umbraco.MacroEngines.Library
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedContent</c>.
    /// </summary>
    /// <remarks>These are dedicated to converting DynamicPublishedContent to INode.</remarks>
	internal static class PublishedContentExtensions
	{		
		internal static INode ConvertToNode(this IPublishedContent doc)
		{
            return CompatibilityHelper.ConvertToNode(doc);
		}
	}
}