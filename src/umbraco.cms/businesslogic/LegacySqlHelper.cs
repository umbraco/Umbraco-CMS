using System;
using System.Configuration;
using umbraco.DataLayer;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace umbraco.cms.businesslogic
{
    [Obsolete("Remove this! This is a temporary class whilst we refactor out old code")]
    internal class LegacySqlHelper
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
                        const string umbracoDsn = Constants.System.UmbracoConnectionName;
                    
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
                        Current.Logger.Error<LegacySqlHelper>(string.Format("Can't instantiate SQLHelper with connectionstring \"{0}\"", connectionString), ex);
                    }
                }

                return _sqlHelper;
            }
        }

    }
}
