using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class DataEditorSettingsStorage
    {
        private ISqlHelper sqlHelper;

        public DataEditorSettingsStorage()
        {
            var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false);

            init(DataLayerHelper.CreateSqlHelper(dataHelper.ConnectionString, false));
        }

        private void init(ISqlHelper connection)
        {
            sqlHelper = connection;
        }

        public List<Setting<string, string>> GetSettings(int dataTypeNodeID)
        {

            string sql = "select * from cmsDataTypePreValues where datatypenodeid = @datatypenodeid";
            IRecordsReader settingsReader = sqlHelper.ExecuteReader(sql, sqlHelper.CreateParameter("@datatypenodeid", dataTypeNodeID));

            List<Setting<string, string>> settings = new List<Setting<string, string>>();

            while (settingsReader.Read())
            {
                Setting<string, string> setting = new Setting<string, string>();
                setting.Key = settingsReader.GetString("alias");
                setting.Value = settingsReader.GetString("value");
                settings.Add(setting);
            }

            settingsReader.Dispose();

            return settings;
        }


        public void ClearSettings(int dataTypeNodeID)
        {
            string sql = "delete from cmsDataTypePreValues where datatypenodeid = @datatypenodeid";
            sqlHelper.ExecuteNonQuery(sql, sqlHelper.CreateParameter("@datatypenodeid", dataTypeNodeID));
        }


        public void InsertSettings(int dataTypeNodeID, List<Setting<string, string>> settings)
        {
            int i = 0;
            foreach (Setting<string, string> s in settings)
            {
                string sql = "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@datatypenodeid,@value,@sortorder,@alias)";
                sqlHelper.ExecuteNonQuery(sql,
                    sqlHelper.CreateParameter("@datatypenodeid", dataTypeNodeID),
                    sqlHelper.CreateParameter("@alias", s.Key),
                    sqlHelper.CreateParameter("@value", s.Value),
                    sqlHelper.CreateParameter("@sortorder", i));

                i++;
            }

        }


        public void InsertSetting(int dataTypeNodeID, string key, string value, int sortOrder)
        {

            string sql = "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@datatypenodeid,@value,@sortorder,@alias)";
            sqlHelper.ExecuteNonQuery(sql,
                sqlHelper.CreateParameter("@datatypenodeid", dataTypeNodeID),
                sqlHelper.CreateParameter("@alias", key),
                sqlHelper.CreateParameter("@value", value),
                sqlHelper.CreateParameter("@sortorder", sortOrder));

        }

        public void UpdateSettings(int dataTypeNodeID, List<Setting<string, string>> settings)
        {
            ClearSettings(dataTypeNodeID);
            InsertSettings(dataTypeNodeID, settings);
        }

        #region IDisposable Members

        public void Dispose()
        {
            sqlHelper.Dispose();
            sqlHelper = null;
        }

        #endregion
    }

    public struct Setting<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
    }
}
