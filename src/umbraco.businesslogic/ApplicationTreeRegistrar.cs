using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    public class ApplicationTreeRegistrar : IApplicationStartupHandler
    {
        public ApplicationTreeRegistrar()
        {
			//don't do anything if the application is not configured!
			if (ApplicationContext.Current == null || !ApplicationContext.Current.IsConfigured)
				return;

            // Load all Trees by attribute and add them to the XML config
			var types = PluginManager.Current.ResolveAttributedTrees();

        	var items = types
        		.Select(x =>
        		        new Tuple<Type, TreeAttribute>(x, x.GetCustomAttributes<TreeAttribute>(false).Single()))
        		.Where(x => ApplicationTree.getByAlias(x.Item2.Alias) == null);

            var allAliases = ApplicationTree.getAll().Select(x => x.Alias).Concat(items.Select(x => x.Item2.Alias));
            var inString = "'" + string.Join("','", allAliases) + "'";

            ApplicationTree.LoadXml(doc =>
            {
                foreach (var tuple in items)
                {
                    var type = tuple.Item1;
                    var attr = tuple.Item2;

                    var typeParts = type.AssemblyQualifiedName.Split(',');
                    var assemblyName = typeParts[1].Trim();
                    var typeName = typeParts[0].Substring(assemblyName.Length + 1).Trim();

                    doc.Root.Add(new XElement("add",
                                              new XAttribute("silent", attr.Silent),
                                              new XAttribute("initialize", attr.Initialize),
                                              new XAttribute("sortOrder", attr.SortOrder),
                                              new XAttribute("alias", attr.Alias),
                                              new XAttribute("application", attr.ApplicationAlias),
                                              new XAttribute("title", attr.Title),
                                              new XAttribute("iconClosed", attr.IconClosed),
                                              new XAttribute("iconOpen", attr.IconOpen),
                                              new XAttribute("assembly", assemblyName),
                                              new XAttribute("type", typeName),
                                              new XAttribute("action", attr.Action)));
                }

                var db = ApplicationContext.Current.DatabaseContext.Database;
                var exist = db.TableExist("umbracoAppTree");
                if (exist)
                {
                    var appTrees = db.Fetch<AppTreeDto>("WHERE treeAlias NOT IN (" + inString + ")");
                    foreach (var appTree in appTrees)
                    {
                        var action = appTree.Action;

                        doc.Root.Add(new XElement("add",
                                                  new XAttribute("silent", appTree.Silent),
                                                  new XAttribute("initialize", appTree.Initialize),
                                                  new XAttribute("sortOrder", appTree.SortOrder),
                                                  new XAttribute("alias", appTree.Alias),
                                                  new XAttribute("application", appTree.AppAlias),
                                                  new XAttribute("title", appTree.Title),
                                                  new XAttribute("iconClosed", appTree.IconClosed),
                                                  new XAttribute("iconOpen", appTree.IconOpen),
                                                  new XAttribute("assembly", appTree.HandlerAssembly),
                                                  new XAttribute("type", appTree.HandlerType),
                                                  new XAttribute("action", string.IsNullOrEmpty(action) ? "" : action)));
                    }
                }

            }, true);
        }
    }
}