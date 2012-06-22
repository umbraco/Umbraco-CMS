/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data.Common;
using System.Reflection;
using umbraco.DataLayer.SqlHelpers.SqlServer;
using umbraco.DataLayer.SqlHelpers.MySql;

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
            /* check arguments */
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            /* try to parse connection string */
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
            try
            {
                connectionStringBuilder.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Bad connection string.", "connectionString", ex);
            }

            // get the data layer type and parse it
            string datalayerType = String.Empty;
            if (connectionStringBuilder.ContainsKey(ConnectionStringDataLayerIdentifier))
            {
                datalayerType = connectionStringBuilder[ConnectionStringDataLayerIdentifier].ToString();
                connectionStringBuilder.Remove(ConnectionStringDataLayerIdentifier);
            }
            
            string[] datalayerTypeParts = datalayerType.Split(",".ToCharArray());
            string helperTypeName = datalayerTypeParts[0].Trim();
            string helperAssemblyName = datalayerTypeParts.Length < 2 ? String.Empty
                                                                      : datalayerTypeParts[1].Trim();
            if (datalayerTypeParts.Length > 2 || (helperTypeName.Length == 0 && helperAssemblyName.Length > 0))
                throw new ArgumentException("Illegal format of data layer property. Should be 'DataLayer = Full_Type_Name [, Assembly_Name]'.", "connectionString");

            /* create the helper */

            // find the right assembly
            Assembly helperAssembly = Assembly.GetExecutingAssembly();
            if (datalayerTypeParts.Length == 2)
            {
                try
                {
                    helperAssembly = Assembly.Load(helperAssemblyName);
                }
                catch (Exception exception)
                {
                    throw new UmbracoException(String.Format("Could not load assembly {0}.", helperAssemblyName), exception);
                }
            }

            // find the right type
            Type helperType;
            if (helperTypeName == String.Empty)
                helperTypeName = DefaultDataHelperName;
            if (!helperTypeName.Contains("."))
                helperTypeName = String.Format(DefaultDataHelperFormat, helperTypeName);
            try
            {
                helperType = helperAssembly.GetType(helperTypeName, true, true);
            }
            catch (Exception exception)
            {
                throw new UmbracoException(String.Format("Could not load type {0} ({1}).", helperTypeName, helperAssembly.FullName), exception);
            }

            // find the right constructor
            ConstructorInfo constructor = helperType.GetConstructor(new Type[] { typeof(string) });
            if (constructor == null)
                throw new UmbracoException(String.Format("Could not find constructor that takes a connection string as parameter. ({0}, {1}).", helperTypeName, helperAssembly.FullName));

            // finally, return the helper
            try
            {
                return constructor.Invoke(new object[] { connectionStringBuilder.ConnectionString }) as ISqlHelper;
            }
            catch (Exception exception)
            {
                throw new UmbracoException(String.Format("Could not execute constructor of type {0} ({1}).", helperTypeName, helperAssembly.FullName), exception);
            }
        }

        public static bool IsEmbeddedDatabase(string connectionString)
        {
            return connectionString.ToLower().Contains("SQLCE4Umbraco.SqlCEHelper".ToLower());
        }

        #endregion
    }   
}
