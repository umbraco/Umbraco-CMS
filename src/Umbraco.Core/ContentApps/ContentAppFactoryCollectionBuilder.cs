﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Web.ContentApps
{
    public class ContentAppFactoryCollectionBuilder : OrderedCollectionBuilderBase<ContentAppFactoryCollectionBuilder, ContentAppFactoryCollection, IContentAppFactory>
    {
        protected override ContentAppFactoryCollectionBuilder This => this;

        // need to inject dependencies in the collection, so override creation
        public override ContentAppFactoryCollection CreateCollection(IServiceProvider serviceProvider)
        {
            // get the logger factory just-in-time - see note below for manifest parser
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var umbracoContextAccessor = serviceProvider.GetRequiredService<IUmbracoContextAccessor>();
            return new ContentAppFactoryCollection(CreateItems(serviceProvider), loggerFactory.CreateLogger<ContentAppFactoryCollection>(), umbracoContextAccessor);
        }

        protected override IEnumerable<IContentAppFactory> CreateItems(IServiceProvider serviceProvider)
        {
            // get the manifest parser just-in-time - injecting it in the ctor would mean that
            // simply getting the builder in order to configure the collection, would require
            // its dependencies too, and that can create cycles or other oddities
            var manifestParser = serviceProvider.GetRequiredService<IManifestParser>();
            var ioHelper = serviceProvider.GetRequiredService<IIOHelper>();
            return base.CreateItems(serviceProvider).Concat(manifestParser.Manifest.ContentApps.Select(x => new ManifestContentAppFactory(x, ioHelper)));
        }
    }
}
