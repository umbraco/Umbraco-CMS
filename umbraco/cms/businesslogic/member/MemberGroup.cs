using System;
using System.Data;
using umbraco.DataLayer;
using System.Collections;

namespace umbraco.cms.businesslogic.member
{
	/// <summary>
	/// Membergroups are used for grouping Umbraco Members
	/// 
	/// A Member can exist in multiple groups.
	/// 
	/// It's possible to protect webpages/documents by membergroup.
	/// </summary>
	public class MemberGroup : CMSNode
	{
		private static Guid _objectType = new Guid("366e63b9-880f-4e13-a61c-98069b029728");

		/// <summary>
		/// Initialize a new object of the MemberGroup class
		/// </summary>
		/// <param name="id">Membergroup id</param>
		public MemberGroup(int id): base(id)
		{

		}

		/// <summary>
		/// Initialize a new object of the MemberGroup class
		/// </summary>
		/// <param name="id">Membergroup id</param>
		public MemberGroup(Guid id) : base(id)
		{
			
		}

        /// <summary>
        /// Deltes the current membergroup
        /// </summary>
        [Obsolete("Use System.Web.Security.Role.DeleteRole")]
        public new void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);
            if (!e.Cancel) {
                // delete member specific data!
                SqlHelper.ExecuteNonQuery("Delete from cmsMember2MemberGroup where memberGroup = @id",
                    SqlHelper.CreateParameter("@id", Id));

                // Delete all content and cmsnode specific data!
                base.delete();
                FireAfterDelete(e);
            }
        }


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {
                FireAfterSave(e);
            }
        }

		/// <summary>
		/// Retrieve a list of all existing MemberGroups
		/// </summary>
		[Obsolete("Use System.Web.Security.Role.GetAllRoles")]
        public static MemberGroup[] GetAll 
		{
			get
			{
				Guid[] tmp = getAllUniquesFromObjectType(_objectType);
				MemberGroup[] retval = new MemberGroup[tmp.Length];

				int i = 0;
				foreach(Guid g in tmp)
				{
					retval[i]= new MemberGroup(g);
					i++;
				}
				return retval;
			}
		}

        public int[] GetMembersAsIds() {
            ArrayList retval = new ArrayList();
            IRecordsReader dr = SqlHelper.ExecuteReader("select member from cmsMember2MemberGroup where memberGroup = @memberGroup",
                SqlHelper.CreateParameter("@memberGroup", Id));
            while (dr.Read()) {
                retval.Add(dr.GetInt("member"));
            }
            dr.Close();

            return (int[])retval.ToArray(typeof(int));
        }

        [Obsolete("Use System.Web.Security.Roles.FindUsersInRole")]
        public Member[] GetMembers() {
            ArrayList retval = new ArrayList();
            IRecordsReader dr = SqlHelper.ExecuteReader("select member from cmsMember2MemberGroup where memberGroup = @memberGroup",
                SqlHelper.CreateParameter("@memberGroup", Id));
            while (dr.Read()) {
                retval.Add(new Member(dr.GetInt("member")));
            }
            dr.Close();

            return (Member[])retval.ToArray(typeof(Member));
        }

        public Member[] GetMembers(string usernameToMatch) {
            ArrayList retval = new ArrayList();
            IRecordsReader dr = SqlHelper.ExecuteReader("select member from cmsMember2MemberGroup inner join cmsMember on cmsMember.nodeId = cmsMember2MemberGroup.member where loginName like @username and memberGroup = @memberGroup",
                SqlHelper.CreateParameter("@memberGroup", Id), SqlHelper.CreateParameter("@username", usernameToMatch + "%"));
            while (dr.Read()) {
                retval.Add(new Member(dr.GetInt("member")));
            }
            dr.Close();

            return (Member[])retval.ToArray(typeof(Member));
        }

        public bool HasMember(int memberId) {
            return SqlHelper.ExecuteScalar<int>("select count(member) from cmsMember2MemberGroup where member = @member and memberGroup = @membergroup",
                SqlHelper.CreateParameter("@member", memberId),
                SqlHelper.CreateParameter("@memberGroup", Id)) > 0; 
        }

		/// <summary>
		/// Get a membergroup by it's name
		/// </summary>
		/// <param name="Name">Name of the membergroup</param>
		/// <returns>If a MemberGroup with the given name exists, it will return this, else: null</returns>
        public static MemberGroup GetByName(string Name) 
		{
			try 
			{
				return
					new MemberGroup(SqlHelper.ExecuteScalar<int>(
								    "select id from umbracoNode where Text = @text and nodeObjectType = @objectType",
								    SqlHelper.CreateParameter("@text", Name),
								    SqlHelper.CreateParameter("@objectType", _objectType)));
			} 
			catch 
			{
				return null;
			}
		}


		/// <summary>
		/// Create a new MemberGroup
		/// </summary>
		/// <param name="Name">The name of the MemberGroup</param>
		/// <param name="u">The creator of the MemberGroup</param>
		/// <returns>The new MemberGroup</returns>
		public static MemberGroup MakeNew(string Name, BusinessLogic.User u) 
		{
			Guid newId = Guid.NewGuid();
			CMSNode.MakeNew(-1,_objectType, u.Id, 1,  Name, newId);
			MemberGroup mg = new MemberGroup(newId);
            NewEventArgs e = new NewEventArgs();
            mg.OnNew(e);
            return mg;
		}

        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(MemberGroup sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(MemberGroup sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(MemberGroup sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when a language is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
	}
}
