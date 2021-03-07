// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events
{
    /// <summary>
    /// Notification raised when Umbraco routes a front-end request.
    /// </summary>
    public class UmbracoRoutedRequest : INotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestBegin"/> class.
        /// </summary>
        public UmbracoRoutedRequest(IUmbracoContext umbracoContext)
        {
            if (!umbracoContext.IsFrontEndUmbracoRequest())
            {
                throw new InvalidOperationException($"{nameof(UmbracoRoutedRequest)} is only valid for Umbraco front-end requests");
            }

            UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// Gets the <see cref="IUmbracoContext"/>
        /// </summary>
        public IUmbracoContext UmbracoContext { get; }
    }
}
