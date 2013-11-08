using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core.Persistence;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;


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

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
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
            var ids = Database.Fetch<int>(
                "select id from cmsTaskType where alias = @0", TypeAlias);
            if (ids.Count == 0) throw new ArgumentException(string.Format("No Task type found for the specified Alias type = '{0}'", TypeAlias));
            _id = ids[0];
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
            Database.FirstOrDefault<string>("select alias from cmsTaskType where id = @id", Id)
                .IfNull<string>(x => { throw new ArgumentException(string.Format("No Task type found for the specified id =  {0}", Id)); })
                .IfNotNull(x => { _id = Id; Alias = x; }); 
        }
        #endregion

        #region Public methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            if (Id == 0)
            {
                // The method is synchronized
                Database.Execute("INSERT INTO cmsTaskType (alias) values (@0)", Alias); 
                Id = Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsTaskType");
            }
            else
            {
                Database.Execute("update cmsTaskType set alias = @0 where id = @1", Alias, Id); 
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
            Database.Execute("DELETE FROM cmsTaskType WHERE id = @0",this._id);
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
            foreach (var taskTypeDto in Database.Query<TaskTypeDto>(sql))
                yield return  new TaskType() { Alias = taskTypeDto.Alias, Id = taskTypeDto.Id };
        }
        #endregion
	
    }
}
