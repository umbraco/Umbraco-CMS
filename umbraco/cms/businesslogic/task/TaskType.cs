using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.BusinessLogic;
using umbraco.DataLayer;


namespace umbraco.cms.businesslogic.task
{
    public class TaskType
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _alias;

        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public TaskType()
        {
        }

        public TaskType(string TypeAlias)
        {
            Id = SqlHelper.ExecuteScalar<int>(
                "select id from cmsTaskType where alias = @alias",
                SqlHelper.CreateParameter("@alias", TypeAlias));
            setup();
        }

        public TaskType(int TaskTypeId)
        {
            Id = TaskTypeId;
            setup();
        }

        private void setup()
        {
			using (IRecordsReader dr =
				SqlHelper.ExecuteReader("select alias from cmsTaskType where id = @id",
				                        SqlHelper.CreateParameter("@id", Id)))
			{
				if(dr.Read())
				{
					_id = Id;
					Alias = dr.GetString("alias");
				}
			}
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            if (Id == 0)
            {
                // The method is synchronized
                SqlHelper.ExecuteNonQuery("INSERT INO cmsTaskType (alias) values (@alias)",
                    SqlHelper.CreateParameter("@alias", Alias));
                Id = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsTaskType");
            }
            else
            {
                SqlHelper.ExecuteNonQuery(
                    "update cmsTaskType set alias = @alias where id = @id",
                    SqlHelper.CreateParameter("@alias", Alias),
                    SqlHelper.CreateParameter("@id", Id));
            }
        }
	
    }
}
