using System;
using System.Linq;
using System.Xml.Linq;
using umbraco.BusinessLogic.Utils;
using umbraco.DataLayer;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    public class ApplicationTreeRegistrar : ApplicationStartupHandler
    {
        private ISqlHelper _sqlHelper;
        protected ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    try
                    {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN);
                    }
                    catch { }
                }
                return _sqlHelper;
            }
        }

        public ApplicationTreeRegistrar()
        {
            // Load all Applications by attribute and add them to the XML config
			var typeFinder = new Umbraco.Core.TypeFinder2();
			var types = typeFinder.FindClassesOfType<ITree>()
                .Where(x => x.GetCustomAttributes(typeof(TreeAttribute), false).Any());

            var items = types.Select(x => new Tuple<Type, TreeAttribute>(x,
                                                                         (TreeAttribute)x.GetCustomAttributes(typeof(TreeAttribute), false).Single()))
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

                var dbTrees = SqlHelper.ExecuteReader("SELECT * FROM umbracoAppTree WHERE treeAlias NOT IN (" + inString + ")");
                while (dbTrees.Read())
                {
                    var action = dbTrees.GetString("action");

                    doc.Root.Add(new XElement("add",
                                              new XAttribute("silent", dbTrees.GetBoolean("treeSilent")),
                                              new XAttribute("initialize", dbTrees.GetBoolean("treeInitialize")),
                                              new XAttribute("sortOrder", dbTrees.GetByte("treeSortOrder")),
                                              new XAttribute("alias", dbTrees.GetString("treeAlias")),
                                              new XAttribute("application", dbTrees.GetString("appAlias")),
                                              new XAttribute("title", dbTrees.GetString("treeTitle")),
                                              new XAttribute("iconClosed", dbTrees.GetString("treeIconClosed")),
                                              new XAttribute("iconOpen", dbTrees.GetString("treeIconOpen")),
                                              new XAttribute("assembly", dbTrees.GetString("treeHandlerAssembly")),
                                              new XAttribute("type", dbTrees.GetString("treeHandlerType")),
                                              new XAttribute("action", string.IsNullOrEmpty(action) ? "" : action)));
                }

            }, true);

            //SqlHelper.ExecuteNonQuery("DELETE FROM umbracoAppTree");
        }
    }
}