using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Web;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;
using umbraco.DataLayer;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Xml;

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
    [Obsolete("Use the MemberService and the Umbraco.Core.Models.Member models instead")]
    public class Member : Content
    {
        #region Constants and static members
        public static readonly string UmbracoMemberProviderName = Constants.Conventions.Member.UmbracoMemberProviderName;
        public static readonly string UmbracoRoleProviderName = Constants.Conventions.Member.UmbracoRoleProviderName;
        public static readonly Guid _objectType = new Guid(Constants.ObjectTypes.Member);

        // zb-00004 #29956 : refactor cookies names & handling

        private const string _sQLOptimizedMany = @"	
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

        private Dictionary<int, IMemberGroup> _groups = null;
        protected internal IMember MemberItem;

        #endregion

        #region Constructors

        internal Member(IMember member)
            : base(member)
        {
            SetupNode(member);
        }

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
        public static Member[] GetAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        public static IEnumerable<Member> GetAllAsList()
        {
            long totalRecs;
            return ApplicationContext.Current.Services.MemberService.GetAll(0, int.MaxValue, out totalRecs)
                .Select(x => new Member(x))
                .ToArray();
        }

        /// <summary>
        /// Retrieves a list of members thats not start with a-z
        /// </summary>
        /// <returns>array of members</returns>
        public static Member[] getAllOtherMembers()
        {

            //NOTE: This hasn't been ported to the new service layer because it is an edge case, it is only used to render the tree nodes but in v7 we plan on 
            // changing how the members are shown and not having to worry about letters.

            var ids = new List<int>();
            using (var dr = SqlHelper.ExecuteReader(
                                        string.Format(_sQLOptimizedMany.Trim(), "LOWER(SUBSTRING(text, 1, 1)) NOT IN ('a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z')", "umbracoNode.text"),
                                            SqlHelper.CreateParameter("@nodeObjectType", Member._objectType)))
            {
                
                while (dr.Read())
                {
                    ids.Add(dr.GetInt("id"));                    
                }
            }

            if (ids.Any())
            {
                return ApplicationContext.Current.Services.MemberService.GetAllMembers(ids.ToArray())
                    .Select(x => new Member(x))
                    .ToArray();
            }

            return new Member[] { };
        }

        /// <summary>
        /// Retrieves a list of members by the first letter in their name.
        /// </summary>
        /// <param name="letter">The first letter</param>
        /// <returns></returns>
        public static Member[] getMemberFromFirstLetter(char letter)
        {
            long totalRecs;

            return ApplicationContext.Current.Services.MemberService.FindMembersByDisplayName(
                letter.ToString(CultureInfo.InvariantCulture), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.StartsWith)
                                     .Select(x => new Member(x))
                                     .ToArray();
        }

        public static Member[] GetMemberByName(string usernameToMatch, bool matchByNameInsteadOfLogin)
        {
            long totalRecs;
            if (matchByNameInsteadOfLogin)
            {
                var found = ApplicationContext.Current.Services.MemberService.FindMembersByDisplayName(
                    usernameToMatch, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.StartsWith);
                return found.Select(x => new Member(x)).ToArray();
            }
            else
            {
                var found = ApplicationContext.Current.Services.MemberService.FindByUsername(
                    usernameToMatch, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.StartsWith);
                return found.Select(x => new Member(x)).ToArray();
            }
        }

        /// <summary>
        /// Creates a new member
        /// </summary>
        /// <param name="Name">Membername</param>
        /// <param name="mbt">Member type</param>
        /// <param name="u">The umbraco usercontext</param>
        /// <returns>The new member</returns>
        public static Member MakeNew(string Name, MemberType mbt, IUser u)
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
        public static Member MakeNew(string Name, string Email, MemberType mbt, IUser u)
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
        public static Member MakeNew(string Name, string LoginName, string Email, MemberType mbt, IUser u)
        {
            if (mbt == null) throw new ArgumentNullException("mbt");            
            var loginName = (string.IsNullOrEmpty(LoginName) == false) ? LoginName : Name;

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            //NOTE: This check is ONLY for backwards compatibility, this check shouldn't really be here it is up to the Membership provider
            // logic to deal with this but it was here before so we can't really change that.
            // Test for e-mail
            if (Email != "" && GetMemberFromEmail(Email) != null && provider.RequiresUniqueEmail)
                throw new Exception(string.Format("Duplicate Email! A member with the e-mail {0} already exists", Email));
            if (GetMemberFromLoginName(loginName) != null)
                throw new Exception(string.Format("Duplicate User name! A member with the user name {0} already exists", loginName));

            var model = ApplicationContext.Current.Services.MemberService.CreateMemberWithIdentity(
                loginName, Email.ToLower(), Name, mbt.MemberTypeItem);

            //The content object will only have the 'WasCancelled' flag set to 'True' if the 'Saving' event has been cancelled, so we return null.
            if (((Entity)model).WasCancelled)
                return null;

            var legacy = new Member(model);
            
            legacy.Save();

            return legacy;
        }

        /// <summary>
        /// Retrieve a member given the loginname
        /// 
        /// Used when authentifying the Member
        /// </summary>
        /// <param name="loginName">The unique Loginname</param>
        /// <returns>The member with the specified loginname - null if no Member with the login exists</returns>
        public static Member GetMemberFromLoginName(string loginName)
        {
            Mandate.ParameterNotNullOrEmpty(loginName, "loginName");

            var found = ApplicationContext.Current.Services.MemberService.GetByUsername(loginName);
            if (found == null) return null;

            return new Member(found);
        }

        /// <summary>
        /// Retrieve a Member given an email, the first if there multiple members with same email
        /// 
        /// Used when authentifying the Member
        /// </summary>
        /// <param name="email">The email of the member</param>
        /// <returns>The member with the specified email - null if no Member with the email exists</returns>
        public static Member GetMemberFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var found = ApplicationContext.Current.Services.MemberService.GetByEmail(email);
            if (found == null) return null;

            return new Member(found);
        }

        /// <summary>
        /// Retrieve Members given an email
        /// 
        /// Used when authentifying a Member
        /// </summary>
        /// <param name="email">The email of the member(s)</param>
        /// <returns>The members with the specified email</returns>
        public static Member[] GetMembersFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            long totalRecs;
            var found = ApplicationContext.Current.Services.MemberService.FindByEmail(
                email, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Exact);

            return found.Select(x => new Member(x)).ToArray();
        }

        /// <summary>
        /// Retrieve a Member given the credentials
        /// 
        /// Used when authentifying the member
        /// </summary>
        /// <param name="loginName">Member login</param>
        /// <param name="password">Member password</param>
        /// <returns>The member with the credentials - null if none exists</returns>
        [Obsolete("Use the MembershipProvider methods to validate a member")]
        public static Member GetMemberFromLoginNameAndPassword(string loginName, string password)
        {
            if (IsMember(loginName))
            {
                var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                // validate user via provider
                if (provider.ValidateUser(loginName, password))
                {
                    return GetMemberFromLoginName(loginName);
                }
                else
                {
                    LogHelper.Debug<Member>("Incorrect login/password attempt or member is locked out or not approved (" + loginName + ")", true);
                    return null;
                }
            }
            else
            {
                LogHelper.Debug<Member>("No member with loginname: " + loginName + " Exists", true);
                return null;
            }
        }

        [Obsolete("This method will not work if the password format is encrypted since the encryption that is performed is not static and a new value will be created each time the same string is encrypted")]
        public static Member GetMemberFromLoginAndEncodedPassword(string loginName, string password)
        {
            var o = SqlHelper.ExecuteScalar<object>(
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

        [Obsolete("Use MembershipProviderExtensions.IsUmbracoMembershipProvider instead")]
        public static bool InUmbracoMemberMode()
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            return provider.IsUmbracoMembershipProvider();
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
            Mandate.ParameterNotNullOrEmpty(loginName, "loginName");
            return ApplicationContext.Current.Services.MemberService.Exists(loginName);
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
            ApplicationContext.Current.Services.MemberService.DeleteMembersOfType(dt.Id);
        }

        #endregion

        #region Public Properties

        public override int sortOrder
        {
            get
            {
                return MemberItem == null ? base.sortOrder : MemberItem.SortOrder;
            }
            set
            {
                if (MemberItem == null)
                {
                    base.sortOrder = value;
                }
                else
                {
                    MemberItem.SortOrder = value;
                }
            }
        }

        public override int Level
        {
            get
            {
                return MemberItem == null ? base.Level : MemberItem.Level;
            }
            set
            {
                if (MemberItem == null)
                {
                    base.Level = value;
                }
                else
                {
                    MemberItem.Level = value;
                }
            }
        }

        public override int ParentId
        {
            get
            {
                return MemberItem == null ? base.ParentId : MemberItem.ParentId;
            }
        }

        public override string Path
        {
            get
            {
                return MemberItem == null ? base.Path : MemberItem.Path;
            }
            set
            {
                if (MemberItem == null)
                {
                    base.Path = value;
                }
                else
                {
                    MemberItem.Path = value;
                }
            }
        }

        [Obsolete("Obsolete, Use Name property on Umbraco.Core.Models.Content", false)]
        public override string Text
        {
            get
            {
                return MemberItem.Name;
            }
            set
            {
                value = value.Trim();
                MemberItem.Name = value;
            }
        }

        /// <summary>
        /// The members password, used when logging in on the public website
        /// </summary>
        [Obsolete("Do not use this property, use GetPassword and ChangePassword instead, if using ChangePassword ensure that the password is encrypted or hashed based on the active membership provider")]
        public string Password
        {
            get
            {
                return MemberItem.RawPasswordValue;
            }
            set
            {
                // We need to use the provider for this in order for hashing, etc. support
                // To write directly to the db use the ChangePassword method
                // this is not pretty but nessecary due to a design flaw (the membership provider should have been a part of the cms project)
                var helper = new MemberShipHelper();
                var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
                MemberItem.RawPasswordValue = helper.EncodePassword(value, provider.PasswordFormat);
            }
        }

        /// <summary>
        /// The loginname of the member, used when logging in
        /// </summary>
        public string LoginName
        {
            get
            {
                return MemberItem.Username;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("The loginname must be different from an empty string", "LoginName");
                if (value.Contains(","))
                    throw new ArgumentException("The parameter 'LoginName' must not contain commas.");
                MemberItem.Username = value;
            }
        }

        /// <summary>
        /// A list of groups the member are member of
        /// </summary>
        public Dictionary<int, IMemberGroup> Groups
        {
            get
            {
                if (_groups == null)
                    PopulateGroups();
                return _groups;
            }
        }

        /// <summary>
        /// The members email
        /// </summary>
        public string Email
        {
            get
            {
                return MemberItem.Email.IsNullOrWhiteSpace() ? string.Empty : MemberItem.Email.ToLower();
            }
            set
            {
                MemberItem.Email = value == null ? "" : value.ToLower();
            }
        }
        #endregion

        #region Public Methods

        [Obsolete("Obsolete", false)]
        protected override void setupNode()
        {
            if (Id == -1)
            {
                base.setupNode();
                return;
            }

            var content = ApplicationContext.Current.Services.MemberService.GetById(Id);

            if (content == null)
                throw new ArgumentException(string.Format("No Member exists with id '{0}'", Id));

            SetupNode(content);
        }

        private void SetupNode(IMember content)
        {
            MemberItem = content;
            //Also need to set the ContentBase item to this one so all the propery values load from it
            ContentBase = MemberItem;

            //Setting private properties from IContentBase replacing CMSNode.setupNode() / CMSNode.PopulateCMSNodeFromReader()
            base.PopulateCMSNodeFromUmbracoEntity(MemberItem, _objectType);

            //If the version is empty we update with the latest version from the current IContent.
            if (Version == Guid.Empty)
                Version = MemberItem.Version;
        }


        /// <summary>
        /// Xmlrepresentation of a member
        /// </summary>
        /// <param name="xd">The xmldocument context</param>
        /// <param name="Deep">Recursive - should always be set to false</param>
        /// <returns>A the xmlrepresentation of the current member</returns>
        public override XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            var x = base.ToXml(xd, Deep);
            if (x.Attributes != null && x.Attributes["loginName"] == null)
            {
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "loginName", LoginName));                
            }
            if (x.Attributes != null && x.Attributes["email"] == null)
            {
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "email", Email));    
            }
            if (x.Attributes != null && x.Attributes["key"] == null)
            {
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "key", UniqueId.ToString()));   
            }                       
            return x;
        }

        /// <summary>
        /// Deltes the current member
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MemberService.Delete()", false)]
        public override void delete()
        {
            if (MemberItem != null)
            {
                ApplicationContext.Current.Services.MemberService.Delete(MemberItem);
            }
            else
            {
                var member = ApplicationContext.Current.Services.MemberService.GetById(Id);
                ApplicationContext.Current.Services.MemberService.Delete(member);
            }

            // Delete all content and cmsnode specific data!
            base.delete();

        }

        /// <summary>
        /// Sets the password for the user - ensure it is encrypted or hashed based on the active membership provider - you must 
        /// call Save() after using this method
        /// </summary>
        /// <param name="newPassword"></param>
        public void ChangePassword(string newPassword)
        {
            MemberItem.RawPasswordValue = newPassword;            
        }

        /// <summary>
        /// Returns the currently stored password - this may be encrypted or hashed string depending on the active membership provider
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            return MemberItem.RawPasswordValue;
        }

        /// <summary>
        /// Adds the member to group with the specified id
        /// </summary>
        /// <param name="GroupId">The id of the group which the member is being added to</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddGroup(int GroupId)
        {
            var parameters = new IParameter[] { SqlHelper.CreateParameter("@id", Id),
                SqlHelper.CreateParameter("@groupId", GroupId) };
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(member) FROM cmsMember2MemberGroup WHERE member = @id AND memberGroup = @groupId",
                parameters) > 0;
            if (!exists)
                SqlHelper.ExecuteNonQuery("INSERT INTO cmsMember2MemberGroup (member, memberGroup) values (@id, @groupId)",
                    parameters);
            PopulateGroups();
            
        }

        /// <summary>
        /// Removes the member from the MemberGroup specified
        /// </summary>
        /// <param name="GroupId">The MemberGroup from which the Member is removed</param>
        public void RemoveGroup(int GroupId)
        {
            SqlHelper.ExecuteNonQuery(
                "delete from cmsMember2MemberGroup where member = @id and Membergroup = @groupId",
                SqlHelper.CreateParameter("@id", Id), SqlHelper.CreateParameter("@groupId", GroupId));
            PopulateGroups();
        }
        #endregion

     

        #region Private methods

        private void PopulateGroups()
        {
            var temp = new Dictionary<int, IMemberGroup>();
            using (var dr = SqlHelper.ExecuteReader(
                "select memberGroup from cmsMember2MemberGroup where member = @id",
                SqlHelper.CreateParameter("@id", Id)))
            {
                while (dr.Read())
                {
                    var group = ApplicationContext.Current.Services.MemberGroupService.GetById(dr.GetInt("memberGroup"));
                    if (group != null)
                    {
                        temp.Add(dr.GetInt("memberGroup"), group);
                    }                    
                }
                    
            }
            _groups = temp;
        }

        private static string GetCacheKey(int id)
        {
            return string.Format("{0}{1}", CacheKeys.MemberBusinessLogicCacheKey, id);
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
        [Obsolete("Use Membership APIs and FormsAuthentication to handle member login")]
        public static void AddMemberToCache(Member m)
        {

            if (m != null)
            {
                // Add cookie with member-id, guid and loginname
                // zb-00035 #29931 : cleanup member state management
                // NH 4.7.1: We'll no longer use legacy cookies to handle Umbraco Members
                //SetMemberState(m);

                FormsAuthentication.SetAuthCookie(m.LoginName, true);

                //cache the member
                var cachedMember = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<Member>(
                    GetCacheKey(m.Id),
                    timeout:        TimeSpan.FromMinutes(30),
                    getCacheItem:   () =>
                    {
                        // Debug information
                        HttpContext.Current.Trace.Write("member",
                            string.Format("Member added to cache: {0}/{1} ({2})",
                                m.Text, m.LoginName, m.Id));

                        return m;
                    });
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
        /// <param name="UseSession">create a persistent cookie</param>
        /// <param name="TimespanForCookie">Has no effect</param>
        [Obsolete("Use the membership api and FormsAuthentication to log users in, this method is no longer used anymore")]
        public static void AddMemberToCache(Member m, bool UseSession, TimeSpan TimespanForCookie)
        {
            if (m != null)
            {
                // zb-00035 #29931 : cleanup member state management
                // NH 4.7.1: We'll no longer use Umbraco legacy cookies to handle members
                //SetMemberState(m, UseSession, TimespanForCookie.TotalDays);

                FormsAuthentication.SetAuthCookie(m.LoginName, !UseSession);

                //cache the member
                var cachedMember = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem<Member>(
                    GetCacheKey(m.Id),
                    timeout:        TimeSpan.FromMinutes(30),
                    getCacheItem:   () =>
                    {
                        // Debug information
                        HttpContext.Current.Trace.Write("member",
                            string.Format("Member added to cache: {0}/{1} ({2})",
                                m.Text, m.LoginName, m.Id));

                        return m;
                    });

            }
        }

        /// <summary>
        /// Removes the member from the cache
        /// 
        /// Can be used in the public website
        /// </summary>
        /// <param name="m">Member to remove</param>
        [Obsolete("Obsolete, use the RemoveMemberFromCache(int NodeId) instead", false)]
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
        [Obsolete("Member cache is automatically cleared when members are updated")]
        public static void RemoveMemberFromCache(int NodeId)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(GetCacheKey(NodeId));
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

            var items = ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItemsByKeySearch<Member>(
                CacheKeys.MemberBusinessLogicCacheKey);
            foreach (var i in items)
            {
                h.Add(i.Id, i);
            }
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
        [Obsolete("Use the standard ASP.Net procedures for hanlding FormsAuthentication, simply check the HttpContext.User and HttpContext.User.Identity.IsAuthenticated to determine if a member is logged in or not")]
        public static bool IsLoggedOn()
        {
            return HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Gets the current visitors memberid
        /// </summary>
        /// <returns>The current visitors members id, if the visitor is not logged in it returns 0</returns>
        public static int CurrentMemberId()
        {
            int currentMemberId = 0;

            // For backwards compatibility between umbraco members and .net membership
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
                var member = provider.GetCurrentUser();
                if (member == null)
                {
                    throw new InvalidOperationException("No member object found with username " + provider.GetCurrentUserName());
                }
                int.TryParse(member.ProviderUserKey.ToString(), out currentMemberId);
            }

            return currentMemberId;
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
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
                    var member = provider.GetCurrentUser();
                    if (member == null)
                    {
                        throw new InvalidOperationException("No member object found with username " + provider.GetCurrentUserName());
                    }

                    int currentMemberId = 0;
                    if (int.TryParse(member.ProviderUserKey.ToString(), out currentMemberId))
                    {
                        var m = new Member(currentMemberId);
                        return m;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Member>("An error occurred in GetCurrentMember", ex);
            }
            return null;
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