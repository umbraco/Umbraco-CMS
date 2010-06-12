using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.task
{
    /// <summary>
    /// An umbraco task is currently only used with the translation workflow in umbraco. But is extendable to cover other taskbased system as well.
    /// A task represent a simple job, it will always be assigned to a user, related to a node, and contain a comment about the task.
    /// The user attached to the task can complete the task, and the author of the task can reopen tasks that are not complete correct.
    /// 
    /// Tasks can in umbraco be used for setting up simple workflows, and contains basic controls structures to determine if the task is completed or not.
    /// </summary>
    public class Task
    {

        #region Properties
        private int _id;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private bool _closed;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Task"/> is closed.
        /// </summary>
        /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
        public bool Closed
        {
            get { return _closed; }
            set { _closed = value; }
        }
	

        private CMSNode _node;

        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        public CMSNode Node
        {
            get { return _node; }
            set { _node = value; }
        }

        private TaskType _type;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public TaskType Type
        {
            get { return _type; }
            set { _type = value; }
        }
	

        private User _parentUser;

        /// <summary>
        /// Gets or sets the parent user.
        /// </summary>
        /// <value>The parent user.</value>
        public User ParentUser
        {
            get { return _parentUser; }
            set { _parentUser = value; }
        }

        private string _comment;

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }
	

        private DateTime _date;

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
	
        private User _user;

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        public User User
        {
            get { return _user; }
            set { _user = value; }
        }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        public Task()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="TaskId">The task id.</param>
        public Task(int TaskId)
        {
			using (IRecordsReader dr =
				SqlHelper.ExecuteReader(
				"select id, taskTypeId, nodeId, parentUserId, userId, DateTime, comment, closed from cmsTask where id = @id",
				SqlHelper.CreateParameter("@id", TaskId)))
			{
                if (dr.Read())
                {
                    PopulateTaskFromReader(dr);
                }
                else
                {
                    throw new ArgumentException("Task with id: '" + TaskId + "' not found");
                }
			}
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes the current task.
        /// Generally tasks should not be deleted and closed instead.
        /// </summary>
        public void Delete()
        {
            SqlHelper.ExecuteNonQuery("DELETE FROM cmsTask WHERE id = @id", SqlHelper.CreateParameter("@id", this._id));
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            //error checking
            if (Node == null) throw new ArgumentNullException("Node");
            if (ParentUser == null) throw new ArgumentNullException("ParentUser");
            if (User == null) throw new ArgumentNullException("User");

            if (Id == 0)
            {
                // The method is synchronized
                SqlHelper.ExecuteNonQuery(
                    "insert into cmsTask (closed, taskTypeId, nodeId, parentUserId, userId, comment) values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @comment)",
                    SqlHelper.CreateParameter("@closed", Closed),
                    SqlHelper.CreateParameter("@taskTypeId", Type.Id),
                    SqlHelper.CreateParameter("@nodeId", Node.Id),
                    SqlHelper.CreateParameter("@parentUserId", ParentUser.Id),
                    SqlHelper.CreateParameter("@userId", User.Id),
                    SqlHelper.CreateParameter("@comment", Comment));
                Id = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsTask");
            }
            else
            {
                SqlHelper.ExecuteNonQuery(
                    "update cmsTask set closed = @closed, taskTypeId = @taskTypeId, nodeId = @nodeId, parentUserId = @parentUserId, userId = @userId, comment = @comment where id = @id",
                    SqlHelper.CreateParameter("@closed", Closed),
                    SqlHelper.CreateParameter("@taskTypeId", Type.Id),
                    SqlHelper.CreateParameter("@nodeId", Node.Id),
                    SqlHelper.CreateParameter("@parentUserId", ParentUser.Id),
                    SqlHelper.CreateParameter("@userId", User.Id),
                    SqlHelper.CreateParameter("@comment", Comment),
                    SqlHelper.CreateParameter("@id", Id));
            }
        }

        #endregion

        #region Private methods
        private void PopulateTaskFromReader(IRecordsReader dr)
        {
            _id = dr.GetInt("id");
            Type = new TaskType((int)dr.GetByte("taskTypeId"));
            Node = new CMSNode(dr.GetInt("nodeId"));
            ParentUser = User.GetUser(dr.GetInt("parentUserId"));
            User = User.GetUser(dr.GetInt("userId"));
            Date = dr.GetDateTime("DateTime");
            Comment = dr.GetString("comment");
            Closed = dr.GetBoolean("closed");
        } 
        #endregion

        #region static methods

        /// <summary>
        /// Returns all tasks by type
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public static Tasks GetTasksByType(int taskType)
        {
            var sql = "select id, taskTypeId, nodeId, parentUserId, userId, DateTime, comment, closed from cmsTask where taskTypeId = @taskTypeId";
            Tasks tasks = new Tasks();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(sql, SqlHelper.CreateParameter("@taskTypeId", taskType)))
            {
                while (dr.Read())
                {
                    var t = new Task();
                    t.PopulateTaskFromReader(dr);
                    tasks.Add(t);
                }
            }

            return tasks;
        }

        /// <summary>
        /// Get all tasks assigned to a node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public static Tasks GetTasks(int nodeId)
        {
            var sql = "select id, taskTypeId, nodeId, parentUserId, userId, DateTime, comment, closed from cmsTask where nodeId = @nodeId";
            Tasks tasks = new Tasks();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(sql, SqlHelper.CreateParameter("@nodeId", nodeId)))
            {
                while (dr.Read())
                {
                    var t = new Task();
                    t.PopulateTaskFromReader(dr);
                    tasks.Add(t);
                }                    
            }

            return tasks;
        }

        /// <summary>
        /// Retrieves a collection of open tasks assigned to the user
        /// </summary>
        /// <param name="User">The User who have the tasks assigned</param>
        /// <param name="IncludeClosed">If true both open and closed tasks will be returned</param>
        /// <returns>A collections of tasks</returns>
        public static Tasks GetTasks(User User, bool IncludeClosed) {
            string sql = "select id, taskTypeId, nodeId, parentUserId, userId, DateTime, comment, closed from cmsTask where userId = @userId";
            if (!IncludeClosed)
                sql += " and closed = 0";
            sql += " order by DateTime desc";

            Tasks tasks = new Tasks();
			using (IRecordsReader dr = SqlHelper.ExecuteReader(
				sql,
				SqlHelper.CreateParameter("@userId", User.Id)))
			{
                while (dr.Read())
                {
                    var t = new Task();
                    t.PopulateTaskFromReader(dr);
                    tasks.Add(t);
                }   				
			}

            return tasks;
        }

        /// <summary>
        /// Retrieves a collection of open tasks assigned to the user
        /// </summary>
        /// <param name="User">The User who have the tasks assigned</param>
        /// <param name="IncludeClosed">If true both open and closed tasks will be returned</param>
        /// <returns>A collections of tasks</returns>
        public static Tasks GetOwnedTasks(User User, bool IncludeClosed) {
            string sql = "select id, taskTypeId, nodeId, parentUserId, userId, DateTime, comment, closed from cmsTask where parentUserId = @userId";
            if (!IncludeClosed)
                sql += " and closed = 0";
            sql += " order by DateTime desc";

            Tasks tasks = new Tasks();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                sql,
                SqlHelper.CreateParameter("@userId", User.Id))) {
                    while (dr.Read())
                    {
                        var t = new Task();
                        t.PopulateTaskFromReader(dr);
                        tasks.Add(t);
                    }   
            }

            return tasks;
        }

        #endregion
    }
}
