using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web;
using System.Xml;
using umbraco.cms.businesslogic.cache;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace umbraco.cms.businesslogic.member
{
    /// <summary>
    /// The Member class represents a member of the public website (not to be confused with umbraco users)
    /// 
    /// Members are used when creating communities and collaborative applications using umbraco, or if there are a 
    /// need for identifying or authentifying the visitor. (extranets, protected/private areas of the public website)
    /// 
    /// Inherits generic datafields from it's baseclass content.
    /// </summary>
    public class Member : Content
    {
        #region Constants and static members
        public static readonly string UmbracoMemberProviderName = "UmbracoMembershipProvider";
        public static readonly string UmbracoRoleProviderName = "UmbracoRoleProvider";
        public static readonly Guid _objectType = new Guid("39eb0f98-b348-42a1-8662-e7eb18487560");

        private static readonly object m_Locker = new object();

        // zb-00004 #29956 : refactor cookies names & handling

        private const string m_SQLOptimizedMany = @"	
			select 
	            umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, 
	            umbracoNode.parentId, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.createDate, 
	            umbracoNode.nodeUser, umbracoNode.text, 
	            cmsMember.Email, cmsMember.LoginName, cmsMember.Password
            from umbracoNode 
            inner join cmsContent on cmsContent.nodeId = umbracoNode.id
            inner join cmsMember on  cmsMember.nodeId = cmsContent.nodeId
			where umbracoNode.nodeObjectType = @nodeObjectType AND {0}			
			order by {1}";

        #endregion

        #region Private members
        private string m_Text;
        private string m_Email;
        private string m_Password;
        private string m_LoginName;
        private Hashtable m_Groups = null;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Member class.
        /// </summary>
        /// <param name="id">Identifier</param>
        public Member(int id) : base(id) { }

        /// <summary>
        /// Initializes a new instance of the Member class.
        /// </summary>
        /// <param name="id">Identifier</param>
        public Member(Guid id) : base(id) { }

        /// <summary>
        /// Initializes a new instance of the Member class, with an option to only initialize 
        /// the data used by the tree in the umbraco console.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="noSetup"></param>
        public Member(int id, bool noSetup) : base(id, noSetup) { }

        public Member(Guid id, bool noSetup) : base(id, noSetup) { }

        #endregion

        #region Static methods
        /// <summary>
        /// A list of all members in the current umbraco install
        /// 
        /// Note: is ressource intensive, use with care.
        /// </summary>
        [Obsolete("Use System.Web.Security.Membership.GetAllUsers()")]
        public static Member[] GetAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        public static IEnumerable<Member> GetAllAsList()
        {
            var tmp = new List<Member>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedMany.Trim(), "1=1", "umbracoNode.text"),
                                            SqlHelper.CreateParameter("@nodeObjectType", Member._objectType)))
            {
                while (dr.Read())
                {
                    Member m = new Member(dr.GetInt("id"), true);
                    m.PopulateMemberFromReader(dr);
                    tmp.Add(m);
                }
            }

            return tmp.ToArray();
        }

        /// <summary>
        /// Retrieves a list of members thats not start with a-z
        /// </summary>
        /// <returns>array of members</returns>
        public static Member[] getAllOtherMembers()
        {

            var tmp = new List<Member>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedMany.Trim(), "(ASCII(SUBSTRING(text, 1, 1)) NOT BETWEEN ASCII('a') AND ASCII('z')) AND (ASCII(SUBSTRING(text, 1, 1)) NOT BETWEEN ASCII('A') AND ASCII('Z'))", "umbracoNode.text"),
                                            SqlHelper.CreateParameter("@nodeObjectType", Member._objectType)))
            {
                while (dr.Read())
                {
                    Member m = new Member(dr.GetInt("id"), true);
                    m.PopulateMemberFromReader(dr);
                    tmp.Add(m);
                }
            }

            return tmp.ToArray();
        }

        /// <summary>
        /// Retrieves a list of members by the first letter in their name.
        /// </summary>
        /// <param name="letter">The first letter</param>
        /// <returns></returns>
        [Obsolete("Use System.Web.Security.Membership.FindUsersByName(string letter)")]
        public static Member[] getMemberFromFirstLetter(char letter)
        {
            return GetMemberByName(letter.ToString(), true);
        }

        [Obsolete("Use System.Web.Security.Membership.FindUsersByName(string letter)")]
        public static Member[] GetMemberByName(string usernameToMatch, bool matchByNameInsteadOfLogin)
        {
            string field = matchByNameInsteadOfLogin ? "umbracoNode.text" : "cmsMember.loginName";

            var tmp = new List<Member>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedMany.Trim(),
                                        string.Format("{0} like @letter", field),
                                        "umbracoNode.text"),
                                            SqlHelper.CreateParameter("@nodeObjectType", Member._objectType),
                                            SqlHelper.CreateParameter("@letter", usernameToMatch + "%")))
            {
                while (dr.Read())
                {
                    Member m = new Member(dr.GetInt("id"), true);
                    m.PopulateMemberFromReader(dr);
                    tmp.Add(m);
                }
            }

            return tmp.ToArray();

        }

        /// <summary>
        /// Creates a new member
        /// </summary>
        /// <param name="Name">Membername</param>
        /// <param name="mbt">Member type</param>
        /// <param name="u">The umbraco usercontext</param>
        /// <returns>The new member</returns>
        [Obsolete("Use System.Web.Security.Membership.CreateUser")]
        public static Member MakeNew(string Name, MemberType mbt, User u)
        {
            return MakeNew(Name, "", "", mbt, u);
        }


        /// <summary>
        /// Creates a new member
        /// </summary>
        /// <param name="Name">Membername</param>
        /// <param name="mbt">Member type</param>
        /// <param name="u">The umbraco usercontext</param>
        /// <param name="Email">The email of the user</param>
        /// <returns>The new member</returns>
        [Obsolete("Use System.Web.Security.Membership.CreateUser")]
        public static Member MakeNew(string Name, string Email, MemberType mbt, User u)
        {
            return MakeNew(Name, "", Email, mbt, u);
        }

        /// <summary>
        /// Creates a new member
        /// </summary>
        /// <param name="Name">Membername</param>
        /// <param name="mbt">Member type</param>
        /// <param name="u">The umbraco usercontext</param>
        /// <param name="Email">The email of the user</param>
        /// <returns>The new member</returns>
        [Obsolete("Use System.Web.Security.Membership.CreateUser")]
        public static Member MakeNew(string Name, string LoginName, string Email, MemberType mbt, User u)
        {
            var loginName = (!String.IsNullOrEmpty(LoginName)) ? LoginName : Name;
            // Test for e-mail
            if (Email != "" && Member.GetMemberFromEmail(Email) != null)
                throw new Exception(String.Format("Duplicate Email! A member with the e-mail {0} already exists", Email));
            else if (Member.GetMemberFromLoginName(LoginName) != null)
                throw new Exception(String.Format("Duplicate User name! A member with the user name {0} already exists", Name));

            Guid newId = Guid.NewGuid();

            //create the cms node first
            CMSNode newNode = MakeNew(-1, _objectType, u.Id, 1, Name, newId);

            //we need to create an empty member and set the underlying text property
            Member tmp = new Member(newId, true);
            tmp.SetText(Name);

            //create the content data for the new member
            tmp.CreateContent(mbt);

            // Create member specific data ..
            SqlHelper.ExecuteNonQuery(
                "insert into cmsMember (nodeId,Email,LoginName,Password) values (@id,@email,@loginName,'')",
                SqlHelper.CreateParameter("@id", tmp.Id),
                SqlHelper.CreateParameter("@loginName", loginName),
                SqlHelper.CreateParameter("@email", Email));

            //read the whole object from the db
            Member m = new Member(newId);

            NewEventArgs e = new NewEventArgs();

            m.OnNew(e);

            m.Save();

            return m;
        }

        /// <summary>
        /// Retrieve a member given the loginname
        /// 
        /// Used when authentifying the Member
        /// </summary>
        /// <param name="loginName">The unique Loginname</param>
        /// <returns>The member with the specified loginname - null if no Member with the login exists</returns>
        [Obsolete("Use System.Web.Security.Membership.GetUser")]
        public static Member GetMemberFromLoginName(string loginName)
        {
            if (IsMember(loginName))
            {
                object o = SqlHelper.ExecuteScalar<object>(
                    "select nodeID from cmsMember where LoginName = @loginName",
                    SqlHelper.CreateParameter("@loginName", loginName));

                if (o == null)
                    return null;

                int tmpId;
                if (!int.TryParse(o.ToString(), out tmpId))
                    return null;

                return new Member(tmpId);
            }
            else
                HttpContext.Current.Trace.Warn("No member with loginname: " + loginName + " Exists");

            return null;
        }

        /// <summary>
        /// Retrieve a Member given an email
        /// 
        /// Used when authentifying the Member
        /// </summary>
        /// <param name="email">The email of the member</param>
        /// <returns>The member with the specified email - null if no Member with the email exists</returns>
        [Obsolete("Use System.Web.Security.Membership.GetUserNameByEmail")]
        public static Member GetMemberFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            object o = SqlHelper.ExecuteScalar<object>(
                "select nodeID from cmsMember where Email = @email",
                SqlHelper.CreateParameter("@email", email));

            if (o == null)
                return null;

            int tmpId;
            if (!int.TryParse(o.ToString(), out tmpId))
                return null;

            return new Member(tmpId);
        }

        /// <summary>
        /// Retrieve a Member given the credentials
        /// 
        /// Used when authentifying the member
        /// </summary>
        /// <param name="loginName">Member login</param>
        /// <param name="password">Member password</param>
        /// <returns>The member with the credentials - null if none exists</returns>
        [Obsolete("Log members in via the standard Forms Authentiaction login")]
        public static Member GetMemberFromLoginNameAndPassword(string loginName, string password)
        {
            if (IsMember(loginName))
            {
                // validate user via provider
                if (Membership.ValidateUser(loginName, password))
                {
                    return GetMemberFromLoginName(loginName);
                }
                else
                {
                    HttpContext.Current.Trace.Warn("Incorrect login/password");
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Trace.Warn("No member with loginname: " + loginName + " Exists");
                //				throw new ArgumentException("No member with Loginname: " + LoginName + " exists");
                return null;
            }
        }

        public static Member GetMemberFromLoginAndEncodedPassword(string loginName, string password)
        {
            object o = SqlHelper.ExecuteScalar<object>(
                "select nodeID from cmsMember where LoginName = @loginName and Password = @password",
                SqlHelper.CreateParameter("loginName", loginName),
                SqlHelper.CreateParameter("password", password));

            if (o == null)
                return null;

            int tmpId;
            if (!int.TryParse(o.ToString(), out tmpId))
                return null;

            return new Member(tmpId);
        }

        public static bool InUmbracoMemberMode()
        {
            return Membership.Provider.Name == UmbracoMemberProviderName;
        }

        public static bool IsUsingUmbracoRoles()
        {
            return Roles.Provider.Name == UmbracoRoleProviderName;
        }


        /// <summary>
        /// Helper method - checks if a Member with the LoginName exists
        /// </summary>
        /// <param name="loginName">Member login</param>
        /// <returns>True if the member exists</returns>
        public static bool IsMember(string loginName)
        {
            Debug.Assert(loginName != null, "loginName cannot be null");
            object o = SqlHelper.ExecuteScalar<object>(
                "select count(nodeID) as tmp from cmsMember where LoginName = @loginName",
                SqlHelper.CreateParameter("@loginName", loginName));
            if (o == null)
                return false;
            int count;
            if (!int.TryParse(o.ToString(), out count))
                return false;
            return count > 0;
        }

        /// <summary>
        /// Deletes all members of the membertype specified
        /// 
        /// Used when a membertype is deleted
        /// 
        /// Use with care
        /// </summary>
        /// <param name="dt">The membertype which are being deleted</param>
        public static void DeleteFromType(MemberType dt)
        {
            var objs = getContentOfContentType(dt);
            foreach (Content c in objs)
            {
                // due to recursive structure document might already been deleted..
                if (IsNode(c.UniqueId))
                {
                    Member tmp = new Member(c.UniqueId);
                    tmp.delete();
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the member
        /// </summary>
        public override string Text
        {
            get
            {
                if (string.IsNullOrEmpty(m_Text))
                {
                    m_Text = SqlHelper.ExecuteScalar<string>(
                        "select text from umbracoNode where id = @id",
                        SqlHelper.CreateParameter("@id", Id));
                }
                return m_Text;
            }
            set
            {
                m_Text = value;
                base.Text = value;
            }
        }

        /// <summary>
        /// The members password, used when logging in on the public website
        /// </summary>
        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(m_Password))
                {
                    m_Password = SqlHelper.ExecuteScalar<string>(
                    "select Password from cmsMember where nodeId = @id",
                    SqlHelper.CreateParameter("@id", Id));
                }
                return m_Password;

            }
            set
            {
                // We need to use the provider for this in order for hashing, etc. support
                // To write directly to the db use the ChangePassword method
                // this is not pretty but nessecary due to a design flaw (the membership provider should have been a part of the cms project)
                MemberShipHelper helper = new MemberShipHelper();
                ChangePassword(helper.EncodePassword(value, Membership.Provider.PasswordFormat));
            }
        }

        /// <summary>
        /// The loginname of the member, used when logging in
        /// </summary>
        public string LoginName
        {
            get
            {
                if (string.IsNullOrEmpty(m_LoginName))
                {
                    m_LoginName = SqlHelper.ExecuteScalar<string>(
                        "select LoginName from cmsMember where nodeId = @id",
                        SqlHelper.CreateParameter("@id", Id));
                }
                return m_LoginName;
            }
            set
            {
                SqlHelper.ExecuteNonQuery(
                    "update cmsMember set LoginName = @loginName where nodeId =  @id",
                    SqlHelper.CreateParameter("@loginName", value),
                    SqlHelper.CreateParameter("@id", Id));
                m_LoginName = value;
            }
        }

        /// <summary>
        /// A list of groups the member are member of
        /// </summary>
        [Obsolete("Use System.Web.Security.Roles.GetRolesForUser()")]
        public Hashtable Groups
        {
            get
            {
                if (m_Groups == null)
                    populateGroups();
                return m_Groups;
            }
        }

        /// <summary>
        /// The members email
        /// </summary>
        public string Email
        {
            get
            {
                return SqlHelper.ExecuteScalar<string>(
                    "select Email from cmsMember where nodeId = @id",
                    SqlHelper.CreateParameter("@id", Id));
            }
            set
            {
                SqlHelper.ExecuteNonQuery(
                    "update cmsMember set Email = @email where nodeId = @id",
                    SqlHelper.CreateParameter("@id", Id), SqlHelper.CreateParameter("@email", value));
            }
        }
        #endregion

        #region Public Methods

        protected override void setupNode()
        {
            base.setupNode();

            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                    @"SELECT Email, LoginName, Password FROM cmsMember WHERE nodeId=@nodeId",
                     SqlHelper.CreateParameter("@nodeId", this.Id)))
            {
                if (dr.Read())
                {
                    if (!dr.IsNull("Email"))
                        m_Email = dr.GetString("Email");
                    m_LoginName = dr.GetString("LoginName");
                    m_Password = dr.GetString("Password");
                }
                else
                {
                    throw new ArgumentException(string.Format("No Member exists with Id '{0}'", this.Id));
                }
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                // re-generate xml
                XmlDocument xd = new XmlDocument();
                XmlGenerate(xd);

                // generate preview for blame history?
                if (UmbracoSettings.EnableGlobalPreviewStorage)
                {
                    // Version as new guid to ensure different versions are generated as members are not versioned currently!
                    SavePreviewXml(generateXmlWithoutSaving(xd), Guid.NewGuid());
                }

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Generates the xmlrepresentation of a member
        /// </summary>
        /// <param name="xd"></param>
        public override void XmlGenerate(XmlDocument xd)
        {
            SaveXmlDocument(generateXmlWithoutSaving(xd));
        }

        /// <summary>
        /// Xmlrepresentation of a member
        /// </summary>
        /// <param name="xd">The xmldocument context</param>
        /// <param name="Deep">Recursive - should always be set to false</param>
        /// <returns>A the xmlrepresentation of the current member</returns>
        public override XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            XmlNode x = base.ToXml(xd, Deep);
            if (x.Attributes["loginName"] == null)
            {
                x.Attributes.Append(xmlHelper.addAttribute(xd, "loginName", LoginName));
                x.Attributes.Append(xmlHelper.addAttribute(xd, "email", Email));
            }
            return x;
        }

        /// <summary>
        /// Deltes the current member
        /// </summary>
        [Obsolete("Use System.Web.Security.Membership.DeleteUser")]
        public override void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                // Remove from cache (if exists)
                Cache.ClearCacheItem(GetCacheKey(Id));

                // delete all relations to groups
                foreach (int groupId in this.Groups.Keys)
                {
                    RemoveGroup(groupId);
                }

                // delete memeberspecific data!
                SqlHelper.ExecuteNonQuery("Delete from cmsMember where nodeId = @id",
                    SqlHelper.CreateParameter("@id", Id));

                // Delete all content and cmsnode specific data!
                base.delete();

                FireAfterDelete(e);
            }
        }

        public void ChangePassword(string newPassword)
        {
            SqlHelper.ExecuteNonQuery(
                    "update cmsMember set Password = @password where nodeId = @id",
                    SqlHelper.CreateParameter("@password", newPassword),
                    SqlHelper.CreateParameter("@id", Id));

            //update this object's password
            m_Password = newPassword;
        }

        /// <summary>
        /// Adds the member to group with the specified id
        /// </summary>
        /// <param name="GroupId">The id of the group which the member is being added to</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        [Obsolete("Use System.Web.Security.Roles.AddUserToRole")]
        public void AddGroup(int GroupId)
        {
            AddGroupEventArgs e = new AddGroupEventArgs();
            e.GroupId = GroupId;
            FireBeforeAddGroup(e);

            if (!e.Cancel)
            {
                IParameter[] parameters = new IParameter[] { SqlHelper.CreateParameter("@id", Id),
                                                         SqlHelper.CreateParameter("@groupId", GroupId) };
                bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(member) FROM cmsMember2MemberGroup WHERE member = @id AND memberGroup = @groupId",
                                                           parameters) > 0;
                if (!exists)
                    SqlHelper.ExecuteNonQuery("INSERT INTO cmsMember2MemberGroup (member, memberGroup) values (@id, @groupId)",
                                              parameters);
                populateGroups();

                FireAfterAddGroup(e);
            }
        }

        /// <summary>
        /// Removes the member from the MemberGroup specified
        /// </summary>
        /// <param name="GroupId">The MemberGroup from which the Member is removed</param>
        [Obsolete("Use System.Web.Security.Roles.RemoveUserFromRole")]
        public void RemoveGroup(int GroupId)
        {
            RemoveGroupEventArgs e = new RemoveGroupEventArgs();
            e.GroupId = GroupId;
            FireBeforeRemoveGroup(e);

            if (!e.Cancel)
            {
                SqlHelper.ExecuteNonQuery(
                    "delete from cmsMember2MemberGroup where member = @id and Membergroup = @groupId",
                    SqlHelper.CreateParameter("@id", Id), SqlHelper.CreateParameter("@groupId", GroupId));
                populateGroups();
                FireAfterRemoveGroup(e);
            }
        }
        #endregion

        #region Protected methods
        protected override XmlNode generateXmlWithoutSaving(XmlDocument xd)
        {
            XmlNode node = xd.CreateNode(XmlNodeType.Element, "node", "");
            XmlPopulate(xd, ref node, false);
            node.Attributes.Append(xmlHelper.addAttribute(xd, "loginName", LoginName));
            node.Attributes.Append(xmlHelper.addAttribute(xd, "email", Email));
            return node;
        }

        protected void PopulateMemberFromReader(IRecordsReader dr)
        {

            SetupNodeForTree(dr.GetGuid("uniqueId"),
                _objectType, dr.GetShort("level"),
                dr.GetInt("parentId"),
                dr.GetInt("nodeUser"),
                dr.GetString("path"),
                dr.GetString("text"),
                dr.GetDateTime("createDate"), false);

            if (!dr.IsNull("Email"))
                m_Email = dr.GetString("Email");
            m_LoginName = dr.GetString("LoginName");
            m_Password = dr.GetString("Password");

        }

        #endregion

        #region Private methods

        private void populateGroups()
        {
            Hashtable temp = new Hashtable();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                "select memberGroup from cmsMember2MemberGroup where member = @id",
                SqlHelper.CreateParameter("@id", Id)))
            {
                while (dr.Read())
                    temp.Add(dr.GetInt("memberGroup"),
                        new MemberGroup(dr.GetInt("memberGroup")));
            }
            m_Groups = temp;
        }

        private static string GetCacheKey(int id)
        {
            return string.Format("MemberCacheItem_{0}", id);
        }

        // zb-00035 #29931 : helper class to handle member state
        class MemberState
        {
            public int MemberId { get; set; }
            public Guid MemberGuid { get; set; }
            public string MemberLogin { get; set; }

            public MemberState(int memberId, Guid memberGuid, string memberLogin)
            {
                MemberId = memberId;
                MemberGuid = memberGuid;
                MemberLogin = memberLogin;
            }
        }

        // zb-00035 #29931 : helper methods to handle member state

        static void SetMemberState(Member member)
        {
            SetMemberState(member.Id, member.UniqueId, member.LoginName);
        }

        static void SetMemberState(int memberId, Guid memberGuid, string memberLogin)
        {
            string value = string.Format("{0}+{1}+{2}", memberId, memberGuid, memberLogin);
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Member.SetValue(value);
        }

        static void SetMemberState(Member member, bool useSession, double cookieDays)
        {
            SetMemberState(member.Id, member.UniqueId, member.LoginName, useSession, cookieDays);
        }

        static void SetMemberState(int memberId, Guid memberGuid, string memberLogin, bool useSession, double cookieDays)
        {
            string value = string.Format("{0}+{1}+{2}", memberId, memberGuid, memberLogin);

            // zb-00004 #29956 : refactor cookies names & handling
            if (useSession)
                HttpContext.Current.Session[StateHelper.Cookies.Member.Key] = value;
            else
                StateHelper.Cookies.Member.SetValue(value, cookieDays);
        }

        static void ClearMemberState()
        {
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Member.Clear();
            FormsAuthentication.SignOut();
        }

        static MemberState GetMemberState()
        {
            // NH: Refactor to fix issue 30171, where auth using pure .NET Members doesn't clear old Umbraco cookie, thus this method gets the previous
            // umbraco user instead of the new one
            // zb-00004 #29956 : refactor cookies names & handling + bring session-related stuff here
            string value = null;
            if (StateHelper.Cookies.Member.HasValue)
            {
                value = StateHelper.Cookies.Member.GetValue();
                if (!String.IsNullOrEmpty(value))
                {
                    string validateMemberId = value.Substring(0, value.IndexOf("+"));
                    if (validateMemberId != Membership.GetUser().ProviderUserKey.ToString())
                    {
                        Member.RemoveMemberFromCache(int.Parse(validateMemberId));
                        value = String.Empty;
                    }
                }
            }

            // compatibility with .NET Memberships
            if (String.IsNullOrEmpty(value) && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                int _currentMemberId = 0;
                if (int.TryParse(Membership.GetUser().ProviderUserKey.ToString(), out _currentMemberId))
                {
                    if (memberExists(_currentMemberId))
                    {
                        // current member is always in the cache, else add it!
                        Member m = GetMemberFromCache(_currentMemberId);
                        if (m == null)
                        {
                            m = new Member(_currentMemberId);
                            AddMemberToCache(m);
                        }
                        return new MemberState(m.Id, m.UniqueId, m.LoginName);
                    }
                }
            }
            else
            {
                var context = HttpContext.Current;
                if (context != null && context.Session != null && context.Session[StateHelper.Cookies.Member.Key] != null)
                {
                    string v = context.Session[StateHelper.Cookies.Member.Key].ToString();
                    if (v != "0")
                        value = v;
                }
            }

            if (value == null)
                return null;

            string[] parts = value.Split(new char[] { '+' });
            if (parts.Length != 3)
                return null;

            int memberId;
            if (!Int32.TryParse(parts[0], out memberId))
                return null;
            Guid memberGuid;
            try
            {
                // Guid.TryParse is in .NET 4 only
                // using try...catch for .NET 3.5 compatibility
                memberGuid = new Guid(parts[1]);
            }
            catch
            {
                return null;
            }

            MemberState ms = new MemberState(memberId, memberGuid, parts[2]);
            return ms;
        }

        #endregion

        #region MemberHandle functions

        /// <summary>
        /// Method is used when logging a member in.
        /// 
        /// Adds the member to the cache of logged in members
        /// 
        /// Uses cookiebased recognition
        /// 
        /// Can be used in the runtime
        /// </summary>
        /// <param name="m">The member to log in</param>
        public static void AddMemberToCache(Member m)
        {

            if (m != null)
            {
                AddToCacheEventArgs e = new AddToCacheEventArgs();
                m.FireBeforeAddToCache(e);

                if (!e.Cancel)
                {
                    // Add cookie with member-id, guid and loginname
                    // zb-00035 #29931 : cleanup member state management
                    SetMemberState(m);

                    //cache the member
                    var cachedMember = Cache.GetCacheItem<Member>(GetCacheKey(m.Id), m_Locker,
                        TimeSpan.FromMinutes(30),
                        delegate
                        {
                            // Debug information
                            HttpContext.Current.Trace.Write("member",
                                string.Format("Member added to cache: {0}/{1} ({2})",
                                    m.Text, m.LoginName, m.Id));

                            return m;
                        });

                    FormsAuthentication.SetAuthCookie(m.LoginName, true);

                    m.FireAfterAddToCache(e);
                }
            }

        }

        // zb-00035 #29931 : remove old cookie code
        /// <summary>
        /// Method is used when logging a member in.
        /// 
        /// Adds the member to the cache of logged in members
        /// 
        /// Uses cookie or session based recognition
        /// 
        /// Can be used in the runtime
        /// </summary>
        /// <param name="m">The member to log in</param>
        /// <param name="UseSession">Use sessionbased recognition</param>
        /// <param name="TimespanForCookie">The live time of the cookie</param>
        public static void AddMemberToCache(Member m, bool UseSession, TimeSpan TimespanForCookie)
        {
            if (m != null)
            {
                AddToCacheEventArgs e = new AddToCacheEventArgs();
                m.FireBeforeAddToCache(e);

                if (!e.Cancel)
                {
                    // zb-00035 #29931 : cleanup member state management
                    SetMemberState(m, UseSession, TimespanForCookie.TotalDays);

                    //cache the member
                    var cachedMember = Cache.GetCacheItem<Member>(GetCacheKey(m.Id), m_Locker,
                        TimeSpan.FromMinutes(30),
                        delegate
                        {
                            // Debug information
                            HttpContext.Current.Trace.Write("member",
                                string.Format("Member added to cache: {0}/{1} ({2})",
                                    m.Text, m.LoginName, m.Id));

                            return m;
                        });


                    FormsAuthentication.SetAuthCookie(m.LoginName, false);

                    m.FireAfterAddToCache(e);
                }

            }
        }

        /// <summary>
        /// Removes the member from the cache
        /// 
        /// Can be used in the public website
        /// </summary>
        /// <param name="m">Member to remove</param>
        [Obsolete("Deprecated, use the RemoveMemberFromCache(int NodeId) instead", false)]
        public static void RemoveMemberFromCache(Member m)
        {
            RemoveMemberFromCache(m.Id);
        }

        /// <summary>
        /// Removes the member from the cache
        /// 
        /// Can be used in the public website
        /// </summary>
        /// <param name="NodeId">Node Id of the member to remove</param>
        public static void RemoveMemberFromCache(int NodeId)
        {
            Cache.ClearCacheItem(GetCacheKey(NodeId));
        }

        /// <summary>
        /// Deletes the member cookie from the browser 
        /// 
        /// Can be used in the public website
        /// </summary>
        /// <param name="m">Member</param>
        [Obsolete("Deprecated, use the ClearMemberFromClient(int NodeId) instead", false)]
        public static void ClearMemberFromClient(Member m)
        {

            if (m != null)
                ClearMemberFromClient(m.Id);
            else
            {
                // If the member doesn't exists as an object, we'll just make sure that cookies are cleared
                // zb-00035 #29931 : cleanup member state management
                ClearMemberState();
            }

            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Deletes the member cookie from the browser 
        /// 
        /// Can be used in the public website
        /// </summary>
        /// <param name="NodeId">The Node id of the member to clear</param>
        public static void ClearMemberFromClient(int NodeId)
        {
            // zb-00035 #29931 : cleanup member state management
            ClearMemberState();
            RemoveMemberFromCache(NodeId);


            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Retrieve a collection of members in the cache
        /// 
        /// Can be used from the public website
        /// </summary>
        /// <returns>A collection of cached members</returns>
        public static Hashtable CachedMembers()
        {
            var h = new Hashtable();
            Cache.ReturnCacheItemsOrdred()
                .Cast<DictionaryEntry>()
                .Where(x => x.Key.ToString().StartsWith("MemberCacheItem_"))
                .Select(x => (Member)x.Value)
                .ToList()
                .ForEach(x =>
                {
                    h.Add(x.Id, x);
                });
            return h;
        }

        /// <summary>
        /// Retrieve a member from the cache
        /// 
        /// Can be used from the public website
        /// </summary>
        /// <param name="id">Id of the member</param>
        /// <returns>If the member is cached it returns the member - else null</returns>
        public static Member GetMemberFromCache(int id)
        {
            Hashtable members = CachedMembers();
            if (members.ContainsKey(id))
                return (Member)members[id];
            else
                return null;
        }

        /// <summary>
        /// An indication if the current visitor is logged in
        /// 
        /// Can be used from the public website
        /// </summary>
        /// <returns>True if the the current visitor is logged in</returns>
        public static bool IsLoggedOn()
        {
            if (HttpContext.Current.User == null)
                return false;


            //if member is not auth'd , but still might have a umb cookie saying otherwise...
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                int _currentMemberId = CurrentMemberId();

                //if we have a cookie... 
                if (_currentMemberId > 0)
                {
                    //log in the member so .net knows about the member.. 
                    FormsAuthentication.SetAuthCookie(new Member(_currentMemberId).LoginName, true);

                    //making sure that the correct status is returned first time around...
                    return true;
                }

            }


            return HttpContext.Current.User.Identity.IsAuthenticated;
        }


        /// <summary>
        /// Make a lookup in the database to verify if a member truely exists
        /// </summary>
        /// <param name="NodeId">The node id of the member</param>
        /// <returns>True is a record exists in db</returns>
        private static bool memberExists(int NodeId)
        {
            return SqlHelper.ExecuteScalar<int>("select count(nodeId) from cmsMember where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", NodeId)) == 1;
        }


        /// <summary>
        /// Gets the current visitors memberid
        /// </summary>
        /// <returns>The current visitors members id, if the visitor is not logged in it returns 0</returns>
        public static int CurrentMemberId()
        {
            int _currentMemberId = 0;

            // For backwards compatibility between umbraco members and .net membership
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                int.TryParse(Membership.GetUser().ProviderUserKey.ToString(), out _currentMemberId);
            }
            else
            {
                // zb-00035 #29931 : cleanup member state management
                MemberState ms = GetMemberState();
                if (ms != null)
                    _currentMemberId = ms.MemberId;
            }

            if (_currentMemberId > 0 && !memberExists(_currentMemberId))
            {
                _currentMemberId = 0;
                // zb-00035 #29931 : cleanup member state management
                ClearMemberState();
            }

            return _currentMemberId;
        }

        /// <summary>
        /// Get the current member
        /// </summary>
        /// <returns>Returns the member, if visitor is not logged in: null</returns>
        public static Member GetCurrentMember()
        {
            try
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    // zb-00035 #29931 : cleanup member state management
                    MemberState ms = GetMemberState();

                    if (ms == null || ms.MemberId == 0)
                        return null;

                    // return member from cache
                    Member member = GetMemberFromCache(ms.MemberId);
                    if (member == null)
                        member = new Member(ms.MemberId);

                    if (HttpContext.Current.User.Identity.IsAuthenticated || (member.UniqueId == ms.MemberGuid && member.LoginName == ms.MemberLogin))
                        return member;
                }
            }
            catch
            {
            }
            return null;
        }

        #endregion

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(Member sender, SaveEventArgs e);

        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(Member sender, NewEventArgs e);

        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(Member sender, DeleteEventArgs e);

        /// <summary>
        /// The add to cache event handler
        /// </summary>
        public delegate void AddingToCacheEventHandler(Member sender, AddToCacheEventArgs e);

        /// <summary>
        /// The add group event handler
        /// </summary>
        public delegate void AddingGroupEventHandler(Member sender, AddGroupEventArgs e);

        /// <summary>
        /// The remove group event handler
        /// </summary>
        public delegate void RemovingGroupEventHandler(Member sender, RemoveGroupEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        new public static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
            {
                BeforeSave(this, e);
            }
        }


        new public static event SaveEventHandler AfterSave;
        new protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
            {
                AfterSave(this, e);
            }
        }


        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
            {
                New(this, e);
            }
        }


        public static event AddingGroupEventHandler BeforeAddGroup;
        protected virtual void FireBeforeAddGroup(AddGroupEventArgs e)
        {
            if (BeforeAddGroup != null)
            {
                BeforeAddGroup(this, e);
            }
        }
        public static event AddingGroupEventHandler AfterAddGroup;
        protected virtual void FireAfterAddGroup(AddGroupEventArgs e)
        {
            if (AfterAddGroup != null)
            {
                AfterAddGroup(this, e);
            }
        }


        public static event RemovingGroupEventHandler BeforeRemoveGroup;
        protected virtual void FireBeforeRemoveGroup(RemoveGroupEventArgs e)
        {
            if (BeforeRemoveGroup != null)
            {
                BeforeRemoveGroup(this, e);
            }
        }

        public static event RemovingGroupEventHandler AfterRemoveGroup;
        protected virtual void FireAfterRemoveGroup(RemoveGroupEventArgs e)
        {
            if (AfterRemoveGroup != null)
            {
                AfterRemoveGroup(this, e);
            }
        }


        public static event AddingToCacheEventHandler BeforeAddToCache;
        protected virtual void FireBeforeAddToCache(AddToCacheEventArgs e)
        {
            if (BeforeAddToCache != null)
            {
                BeforeAddToCache(this, e);
            }
        }


        public static event AddingToCacheEventHandler AfterAddToCache;
        protected virtual void FireAfterAddToCache(AddToCacheEventArgs e)
        {
            if (AfterAddToCache != null)
            {
                AfterAddToCache(this, e);
            }
        }

        new public static event DeleteEventHandler BeforeDelete;
        new protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
            {
                BeforeDelete(this, e);
            }
        }

        new public static event DeleteEventHandler AfterDelete;
        new protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
            {
                AfterDelete(this, e);
            }
        }
        #endregion

        #region Membership helper class used for encryption methods
        /// <summary>
        /// ONLY FOR INTERNAL USE.
        /// This is needed due to a design flaw where the Umbraco membership provider is located 
        /// in a separate project referencing this project, which means we can't call special methods
        /// directly on the UmbracoMemberShipMember class.
        /// This is a helper implementation only to be able to use the encryption functionality 
        /// of the membership provides (which are protected).
        /// 
        /// ... which means this class should have been marked internal with a Friend reference to the other assembly right??
        /// </summary>
        internal class MemberShipHelper : MembershipProvider
        {
            public override string ApplicationName
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public override bool ChangePassword(string username, string oldPassword, string newPassword)
            {
                throw new NotImplementedException();
            }

            public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
            {
                throw new NotImplementedException();
            }

            public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteUser(string username, bool deleteAllRelatedData)
            {
                throw new NotImplementedException();
            }

            public string EncodePassword(string password, MembershipPasswordFormat pwFormat)
            {
                string encodedPassword = password;
                switch (pwFormat)
                {
                    case MembershipPasswordFormat.Clear:
                        break;
                    case MembershipPasswordFormat.Encrypted:
                        encodedPassword =
                          Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                        break;
                    case MembershipPasswordFormat.Hashed:
                        HMACSHA1 hash = new HMACSHA1();
                        hash.Key = Encoding.Unicode.GetBytes(password);
                        encodedPassword =
                          Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                        break;
                }
                return encodedPassword;
            }

            public override bool EnablePasswordReset
            {
                get { throw new NotImplementedException(); }
            }

            public override bool EnablePasswordRetrieval
            {
                get { throw new NotImplementedException(); }
            }

            public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            public override int GetNumberOfUsersOnline()
            {
                throw new NotImplementedException();
            }

            public override string GetPassword(string username, string answer)
            {
                throw new NotImplementedException();
            }

            public override MembershipUser GetUser(string username, bool userIsOnline)
            {
                throw new NotImplementedException();
            }

            public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
            {
                throw new NotImplementedException();
            }

            public override string GetUserNameByEmail(string email)
            {
                throw new NotImplementedException();
            }

            public override int MaxInvalidPasswordAttempts
            {
                get { throw new NotImplementedException(); }
            }

            public override int MinRequiredNonAlphanumericCharacters
            {
                get { throw new NotImplementedException(); }
            }

            public override int MinRequiredPasswordLength
            {
                get { throw new NotImplementedException(); }
            }

            public override int PasswordAttemptWindow
            {
                get { throw new NotImplementedException(); }
            }

            public override MembershipPasswordFormat PasswordFormat
            {
                get { throw new NotImplementedException(); }
            }

            public override string PasswordStrengthRegularExpression
            {
                get { throw new NotImplementedException(); }
            }

            public override bool RequiresQuestionAndAnswer
            {
                get { throw new NotImplementedException(); }
            }

            public override bool RequiresUniqueEmail
            {
                get { throw new NotImplementedException(); }
            }

            public override string ResetPassword(string username, string answer)
            {
                throw new NotImplementedException();
            }

            public override bool UnlockUser(string userName)
            {
                throw new NotImplementedException();
            }

            public override void UpdateUser(MembershipUser user)
            {
                throw new NotImplementedException();
            }

            public override bool ValidateUser(string username, string password)
            {
                throw new NotImplementedException();
            }
        }
        #endregion

    }


}