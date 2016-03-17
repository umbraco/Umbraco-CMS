using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.DataLayer;


namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Class for handling all registered applications in Umbraco.
    /// </summary>
    [Obsolete("Use ApplicationContext.Current.Services.SectionService and/or Umbraco.Core.Sections.SectionCollection instead")]
    public class LegacySqlHelper
    {
        private static ISqlHelper _sqlHelper;
        
        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Do not use SqlHelper anymore, if database querying needs to be done use the DatabaseContext instead")]
        public static ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    var connectionString = string.Empty;

                    try
                    {
                        const string umbracoDsn = Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName;
                    
                        var databaseSettings = ConfigurationManager.ConnectionStrings[umbracoDsn];
                        if (databaseSettings != null)
                            connectionString = databaseSettings.ConnectionString;

                        // During upgrades we might still have the old appSettings connectionstring, and not the new one, so get that one instead
                        if (string.IsNullOrWhiteSpace(connectionString) &&
                            ConfigurationManager.AppSettings.ContainsKey(umbracoDsn))
                            connectionString = ConfigurationManager.AppSettings[umbracoDsn];

                        _sqlHelper = DataLayerHelper.CreateSqlHelper(connectionString, false);
                    }
                    catch(Exception ex)
                    {
                        LogHelper.Error<LegacySqlHelper>(string.Format("Can't instantiate SQLHelper with connectionstring \"{0}\"", connectionString), ex);
                    }
                }

                return _sqlHelper;
            }
        }

    }
}
