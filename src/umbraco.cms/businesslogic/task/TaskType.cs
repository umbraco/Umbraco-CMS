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
        

        #region Public Properties

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

        /// <summary>
        /// All tasks associated with this task type
        /// </summary>
        public Tasks Tasks
        {
            get
            {
                //lazy load the tasks
                if (m_Tasks == null)
                {
                    m_Tasks = Task.GetTasksByType(this.Id);
                }
                return m_Tasks;
            }
        }

        #endregion

        #region Private/protected members

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        private int _id;
        private Tasks m_Tasks;

        #endregion

        #region Constructors
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
        #endregion

        #region Private methods
        private void setup()         
        {
			using (IRecordsReader dr =
				SqlHelper.ExecuteReader("select alias from cmsTaskType where id = @id",
				                        SqlHelper.CreateParameter("@id", Id)))
			{
                if (dr.Read())
                {
                    _id = Id;
                    Alias = dr.GetString("alias");
                }
                else
                {
                    throw new ArgumentException("No Task type found for the id specified");
                }
			}
        }
        #endregion

        #region Public methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            if (Id == 0)
            {
                // The method is synchronized
                SqlHelper.ExecuteNonQuery("INSERT INTO cmsTaskType (alias) values (@alias)",
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

        /// <summary>
        /// Deletes the current task type.
        /// This will remove all tasks associated with this type
        /// </summary>
        public void Delete()
        {
            foreach (Task t in Tasks)
            {
                t.Delete();
            }
            m_Tasks.Clear(); //remove the internal collection
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTaskType WHERE id = @id", SqlHelper.CreateParameter("@id", this._id));
        } 

        #endregion

        #region Static methods
        /// <summary>
        /// Returns all task types stored in the database
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TaskType> GetAll()
        {
            var sql = "SELECT id, alias FROM cmsTaskType";
            var types = new List<TaskType>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(sql))
            {
                while (dr.Read())
                {
                    types.Add(new TaskType() { _alias = dr.GetString("alias"), _id = Convert.ToInt32(dr.Get<object>("id")) });
                }
            }
            return types;
        } 
        #endregion
	
    }
}
