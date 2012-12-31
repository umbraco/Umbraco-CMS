using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using umbraco.BusinessLogic.Utils;
using umbraco.DataLayer;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    public class ApplicationRegistrar : IApplicationStartupHandler
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
                        var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString);
                    }
                    catch { }
                }
                return _sqlHelper;
            }
        }

        public ApplicationRegistrar()
        {

			//don't do anything if the application is not configured!
			if (!ApplicationContext.Current.IsConfigured)
				return;

            // Load all Applications by attribute and add them to the XML config
        	var types = PluginManager.Current.ResolveApplications();

			//since applications don't populate their metadata from the attribute and because it is an interface, 
			//we need to interrogate the attributes for the data. Would be better to have a base class that contains 
			//metadata populated by the attribute. Oh well i guess.
			var attrs = types.Select(x => x.GetCustomAttributes<ApplicationAttribute>(false).Single())
                .Where(x => Application.getByAlias(x.Alias) == null);

            var allAliases = Application.getAll().Select(x => x.alias).Concat(attrs.Select(x => x.Alias));
            var inString = "'" + string.Join("','", allAliases) + "'";
			
            Application.LoadXml(doc =>
                {
                    foreach (var attr in attrs)
                    {
                        doc.Root.Add(new XElement("add",
                                                  new XAttribute("alias", attr.Alias),
                                                  new XAttribute("name", attr.Name),
                                                  new XAttribute("icon", attr.Icon),
                                                  new XAttribute("sortOrder", attr.SortOrder)));
                    }
                    
                    var db = ApplicationContext.Current.DatabaseContext.Database;
                    var exist = db.TableExist("umbracoApp");
                    if (exist)
                    {
                        var dbApps = SqlHelper.ExecuteReader("SELECT * FROM umbracoApp WHERE appAlias NOT IN (" + inString + ")");
                        while (dbApps.Read())
                        {
                            doc.Root.Add(new XElement("add",
                                                      new XAttribute("alias", dbApps.GetString("appAlias")),
                                                      new XAttribute("name", dbApps.GetString("appName")),
                                                      new XAttribute("icon", dbApps.GetString("appIcon")),
                                                      new XAttribute("sortOrder", dbApps.GetByte("sortOrder"))));
                        }
                    }

                }, true);
            
            //TODO Shouldn't this be enabled and then delete the whole table?
            //SqlHelper.ExecuteNonQuery("DELETE FROM umbracoApp");
        }
    }
}