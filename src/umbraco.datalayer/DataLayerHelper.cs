/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Configuration;
using System.Data.Common;
using System.Reflection;
using Umbraco.Core;

namespace umbraco.DataLayer
{
    /// <summary>
    /// The DataLayerHelper class is the main interface to the data layer.
    /// </summary>
    public class DataLayerHelper
    {
        #region Private Constants

        /// <summary>Name of the property that identifies the SQL helper type.</summary>
        private const string ConnectionStringDataLayerIdentifier = "datalayer";
        /// <summary>Name of the default data layer, that is used when nothing is specified.</summary>
        private const string DefaultDataHelperName = "SqlServer";
        /// <summary>Format used when the SQL helper is qualified by its simple name, instead of the full class name.</summary>
        private const string DefaultDataHelperFormat = "umbraco.DataLayer.SqlHelpers.{0}.{0}Helper";

        private static string _dataHelperTypeName;
        private static string _dataHelperAssemblyName;
        private static string _connectionString;

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a SQL helper for the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string containing the SQL helper type.</param>
        /// <returns>A new SQL helper.</returns>
        /// <remarks>This method will change to allow the addition of external SQL helpers.</remarks>
        public static ISqlHelper CreateSqlHelper(string connectionString)
        {
            return CreateSqlHelper(connectionString, true);
        }

        public static ISqlHelper CreateSqlHelper(string connectionString, bool forceLegacyConnection)
        {
            /* check arguments */
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            if (forceLegacyConnection == false && IsEmbeddedDatabase(connectionString) && connectionString.ToLower().Contains("SQLCE4Umbraco".ToLower()) == false)
            {
                connectionString = connectionString.Replace("Datasource", "Data Source");

                if(connectionString.Contains(@"|\") == false)
                connectionString = connectionString.Insert(connectionString.LastIndexOf('|') + 1, "\\");

                connectionString = string.Format("datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;{0}", connectionString);
            }

            /* try to parse connection string */
            var connectionStringBuilder = new DbConnectionStringBuilder();
            try
            {
                connectionStringBuilder.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Bad connection string.", "connectionString", ex);
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
            
            if (forceLegacyConnection == false && connectionStringSettings != null)
                SetDataHelperNames(connectionStringSettings);
            else
                SetDataHelperNamesLegacyConnectionString(connectionStringBuilder);

            /* create the helper */
            // find the right assembly
            var helperAssembly = Assembly.GetExecutingAssembly();
            if (string.IsNullOrWhiteSpace(_dataHelperAssemblyName) == false)
            {
                try
                {
                    helperAssembly = Assembly.Load(_dataHelperAssemblyName);
                }
                catch (Exception exception)
                {
                    throw new UmbracoException(String.Format("Could not load assembly {0}.", _dataHelperAssemblyName), exception);
                }
            }

            // find the right type
            Type helperType;
            if (string.IsNullOrWhiteSpace(_dataHelperTypeName))
                _dataHelperTypeName = DefaultDataHelperName;

            if (_dataHelperTypeName.Contains(".") == false)
                _dataHelperTypeName = string.Format(DefaultDataHelperFormat, _dataHelperTypeName);

            try
            {
                helperType = helperAssembly.GetType(_dataHelperTypeName, true, true);
            }
            catch (Exception exception)
            {
                throw new UmbracoException(String.Format("Could not load type {0} ({1}).", _dataHelperTypeName, helperAssembly.FullName), exception);
            }

            // find the right constructor
            var constructor = helperType.GetConstructor(new[] { typeof(string) });
            if (constructor == null)
                throw new UmbracoException(String.Format("Could not find constructor that takes a connection string as parameter. ({0}, {1}).", _dataHelperTypeName, helperAssembly.FullName));

            // finally, return the helper
            try
            {
                return constructor.Invoke(new object[] { _connectionString }) as ISqlHelper;
            }
            catch (Exception exception)
            {
                throw new UmbracoException(String.Format("Could not execute constructor of type {0} ({1}).", _dataHelperTypeName, helperAssembly.FullName), exception);
            }
        }

        private static void SetDataHelperNames(ConnectionStringSettings connectionStringSettings)
        {
            _connectionString = connectionStringSettings.ConnectionString;

            var provider = connectionStringSettings.ProviderName;

            if (provider.StartsWith("MySql"))
            {
                _dataHelperTypeName = "MySql";
            }

            if (provider.StartsWith("System.Data.SqlServerCe"))
            {
                _dataHelperTypeName = "SQLCE4Umbraco.SqlCEHelper";
                _dataHelperAssemblyName = "SQLCE4Umbraco";
            }
        }

        private static void SetDataHelperNamesLegacyConnectionString(DbConnectionStringBuilder connectionStringBuilder)
        {
            // get the data layer type and parse it
            var datalayerType = String.Empty;
            if (connectionStringBuilder.ContainsKey(ConnectionStringDataLayerIdentifier))
            {
                datalayerType = connectionStringBuilder[ConnectionStringDataLayerIdentifier].ToString();
                connectionStringBuilder.Remove(ConnectionStringDataLayerIdentifier);
            }

            _connectionString = connectionStringBuilder.ConnectionString;

            var datalayerTypeParts = datalayerType.Split(",".ToCharArray());

            _dataHelperTypeName = datalayerTypeParts[0].Trim();
            _dataHelperAssemblyName = datalayerTypeParts.Length < 2
                                          ? string.Empty
                                          : datalayerTypeParts[1].Trim();

            if (datalayerTypeParts.Length > 2 || (_dataHelperTypeName.Length == 0 && _dataHelperAssemblyName.Length > 0))
                throw new ArgumentException("Illegal format of data layer property. Should be 'DataLayer = Full_Type_Name [, Assembly_Name]'.", "connectionString");
        }

        public static bool IsEmbeddedDatabase(string connectionString)
        {
            return connectionString.ToLower().Contains("|DataDirectory|".ToLower());
        }

        #endregion
    }
}
