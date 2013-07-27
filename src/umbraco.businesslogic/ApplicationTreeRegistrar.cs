using System;
using System.Linq;
using System.Xml.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{

    //TODO: We should move this to Core but currently the legacy tree attribute exists here, perhaps we need two startup handlers
    // one to register the legacy ones and one to register the new ones?? And create a new attribute

    /// <summary>
    /// A startup handler for dealing with trees
    /// </summary>
    public class ApplicationTreeRegistrar : ApplicationEventHandler, IMapperConfiguration
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ScanTrees(applicationContext);
        }

        /// <summary>
        /// Configures automapper model mappings
        /// </summary>
        public void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Umbraco.Core.Models.ApplicationTree, ApplicationTree>()
                  .ReverseMap(); //two way
        }

        /// <summary>
        /// Scans for all attributed trees and ensures they exist in the tree xml
        /// </summary>
        private static void ScanTrees(ApplicationContext applicationContext)
        {
            // Load all Trees by attribute and add them to the XML config
            var types = PluginManager.Current.ResolveAttributedTrees();

            var items = types
                .Select(x =>
                        new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
                .Where(x => applicationContext.Services.ApplicationTreeService.GetByAlias(x.Item2.Alias) == null);

            applicationContext.Services.ApplicationTreeService.LoadXml(doc =>
                {
                    foreach (var tuple in items)
                    {
                        var type = tuple.Item1;
                        var attr = tuple.Item2;

                        //Add the new tree that doesn't exist in the config that was found by type finding

                        doc.Root.Add(new XElement("add",
                                                  new XAttribute("silent", attr.Silent),
                                                  new XAttribute("initialize", attr.Initialize),
                                                  new XAttribute("sortOrder", attr.SortOrder),
                                                  new XAttribute("alias", attr.Alias),
                                                  new XAttribute("application", attr.ApplicationAlias),
                                                  new XAttribute("title", attr.Title),
                                                  new XAttribute("iconClosed", attr.IconClosed),
                                                  new XAttribute("iconOpen", attr.IconOpen),
                                                  new XAttribute("type", type.GetFullNameWithAssembly()),
                                                  new XAttribute("action", attr.Action)));
                    }

                }, true);
        }

    }
}
 