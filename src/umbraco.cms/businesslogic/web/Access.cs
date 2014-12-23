using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using System.IO;

using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Security;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for Access.
    /// </summary>
    public class Access
    {
        static private readonly Hashtable CheckedPages = new Hashtable();

        static private volatile XmlDocument _accessXmlContent;
        static private string _accessXmlFilePath;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static readonly object LoadLocker = new object();

        [Obsolete("Do not access this property directly, it is not thread safe, use GetXmlDocumentCopy instead")]
        public static XmlDocument AccessXml
        {
            get { return GetXmlDocument(); }
        }

        //private method to initialize and return the in-memory xmldocument
        private static XmlDocument GetXmlDocument()
        {
            if (_accessXmlContent == null)
            {
                lock (LoadLocker)
                {
                    if (_accessXmlContent == null)
                    {
                        if (_accessXmlFilePath == null)
                        {
                            //if we pop it here it'll make for better stack traces ;)
                            _accessXmlFilePath = IOHelper.MapPath(SystemFiles.AccessXml);
                        }

                        _accessXmlContent = new XmlDocument();

                        if (File.Exists(_accessXmlFilePath) == false)
                        {
                            var file = new FileInfo(_accessXmlFilePath);
                            if (Directory.Exists(file.DirectoryName) == false)
                            {
                                Directory.CreateDirectory(file.Directory.FullName); //ensure the folder exists!	
                            }
                            var f = File.Open(_accessXmlFilePath, FileMode.Create);
                            var sw = new StreamWriter(f);
                            sw.WriteLine("<access/>");
                            sw.Close();
                            f.Close();
                        }
                        _accessXmlContent.Load(_accessXmlFilePath);
                    }
                }
            }
            return _accessXmlContent;
        }

        //used by all other methods in this class to read and write the document which 
        // is thread safe and only clones once per request so it's still fast.
        public static XmlDocument GetXmlDocumentCopy()
        {
            if (HttpContext.Current == null)
            {
                return (XmlDocument)GetXmlDocument().Clone();    
            }

            if (HttpContext.Current.Items.Contains(typeof (Access)) == false)
            {
                HttpContext.Current.Items.Add(typeof (Access), GetXmlDocument().Clone());
            }

            return (XmlDocument)HttpContext.Current.Items[typeof(Access)];
        }

        #region Manipulation methods

        public static void AddMembershipRoleToDocument(int documentId, string role)
        {
            //event
            var e = new AddMemberShipRoleToDocumentEventArgs();
            new Access().FireBeforeAddMemberShipRoleToDocument(new Document(documentId), role, e);

            if (e.Cancel) return;

            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(documentId);

                if (x == null)
                    throw new Exception("Document is not protected!");

                if (x.SelectSingleNode("group [@id = '" + role + "']") == null)
                {
                    var groupXml = (XmlElement)x.OwnerDocument.CreateNode(XmlNodeType.Element, "group", "");
                    groupXml.SetAttribute("id", role);
                    x.AppendChild(groupXml);
                    Save(x.OwnerDocument);
                }
            }

            new Access().FireAfterAddMemberShipRoleToDocument(new Document(documentId), role, e);
        }
        
        /// <summary>
        /// Used to refresh cache among servers in an LB scenario
        /// </summary>
        /// <param name="newDoc"></param>
        internal static void UpdateInMemoryDocument(XmlDocument newDoc)
        {
            //NOTE: This would be better to use our normal ReaderWriter lock but because we are emitting an 
            // event inside of the WriteLock and code can then listen to the event and call this method we end
            // up in a dead-lock. This specifically happens in the PublicAccessCacheRefresher.
            //So instead we use the load locker which is what is used for the static XmlDocument instance, we'll 
            // lock that, set the doc to null which will cause any reader threads to block for the AccessXml instance
            // then save the doc and re-load it, then all blocked threads can carry on.
            lock (LoadLocker)
            {
                _accessXmlContent = null;
                //do a real clone
                _accessXmlContent = new XmlDocument();
                _accessXmlContent.LoadXml(newDoc.OuterXml);
                ClearCheckPages();
            }
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void AddMemberGroupToDocument(int DocumentId, int MemberGroupId)
        {
            var x = (XmlElement)GetPage(DocumentId);

            if (x == null)
                throw new Exception("Document is not protected!");

            using (new WriteLock(Locker))
            {
                if (x.SelectSingleNode("group [@id = '" + MemberGroupId + "']") == null)
                {
                    var groupXml = (XmlElement)x.OwnerDocument.CreateNode(XmlNodeType.Element, "group", "");
                    groupXml.SetAttribute("id", MemberGroupId.ToString(CultureInfo.InvariantCulture));
                    x.AppendChild(groupXml);
                    Save(x.OwnerDocument);
                }
            }
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void AddMemberToDocument(int DocumentId, int MemberId)
        {
            var x = (XmlElement)GetPage(DocumentId);

            if (x == null)
                throw new Exception("Document is not protected!");

            using (new WriteLock(Locker))
            {
                if (x.Attributes.GetNamedItem("memberId") != null)
                {
                    x.Attributes.GetNamedItem("memberId").Value = MemberId.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    x.SetAttribute("memberId", MemberId.ToString(CultureInfo.InvariantCulture));
                }
                Save(x.OwnerDocument);
            }
        }

        public static void AddMembershipUserToDocument(int documentId, string membershipUserName)
        {
            //event
            var e = new AddMembershipUserToDocumentEventArgs();
            new Access().FireBeforeAddMembershipUserToDocument(new Document(documentId), membershipUserName, e);

            if (e.Cancel) return;

            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(documentId);

                if (x == null)
                    throw new Exception("Document is not protected!");

                if (x.Attributes.GetNamedItem("memberId") != null)
                    x.Attributes.GetNamedItem("memberId").Value = membershipUserName;
                else
                    x.SetAttribute("memberId", membershipUserName);
                Save(x.OwnerDocument);
            }

            new Access().FireAfterAddMembershipUserToDocument(new Document(documentId), membershipUserName, e);
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void RemoveMemberGroupFromDocument(int DocumentId, int MemberGroupId)
        {
            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(DocumentId);

                if (x == null)
                    throw new Exception("Document is not protected!");

                var xGroup = x.SelectSingleNode("group [@id = '" + MemberGroupId + "']");
                if (xGroup == null) return;

                x.RemoveChild(xGroup);
                Save(x.OwnerDocument);
            }
        }

        public static void RemoveMembershipRoleFromDocument(int documentId, string role)
        {
            var e = new RemoveMemberShipRoleFromDocumentEventArgs();
            new Access().FireBeforeRemoveMemberShipRoleFromDocument(new Document(documentId), role, e);

            if (e.Cancel) return;

            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(documentId);
                if (x == null)
                    throw new Exception("Document is not protected!");
                var xGroup = x.SelectSingleNode("group [@id = '" + role + "']");

                if (xGroup != null)
                {
                    x.RemoveChild(xGroup);
                    Save(x.OwnerDocument);
                }
            }

            new Access().FireAfterRemoveMemberShipRoleFromDocument(new Document(documentId), role, e);
        }

        public static bool RenameMemberShipRole(string oldRolename, string newRolename)
        {
            var hasChange = false;
            if (oldRolename == newRolename) return false;

            using (new WriteLock(Locker))
            {
                var xDoc = GetXmlDocumentCopy();
                oldRolename = oldRolename.Replace("'", "&apos;");
                foreach (XmlNode x in xDoc.SelectNodes("//group [@id = '" + oldRolename + "']"))
                {
                    x.Attributes["id"].Value = newRolename;
                    hasChange = true;
                }
                if (hasChange)
                    Save(xDoc);
            }

            return hasChange;
        }

        public static void ProtectPage(bool Simple, int DocumentId, int LoginDocumentId, int ErrorDocumentId)
        {
            var e = new AddProtectionEventArgs();
            new Access().FireBeforeAddProtection(new Document(DocumentId), e);

            if (e.Cancel) return;

            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(DocumentId);

                if (x == null)
                {
                    var xDoc = GetXmlDocumentCopy();

                    x = (XmlElement)xDoc.CreateNode(XmlNodeType.Element, "page", "");
                    x.OwnerDocument.DocumentElement.AppendChild(x);
                }
                // if using simple mode, make sure that all existing groups are removed
                else if (Simple)
                {
                    x.RemoveAll();
                }
                x.SetAttribute("id", DocumentId.ToString());
                x.SetAttribute("loginPage", LoginDocumentId.ToString());
                x.SetAttribute("noRightsPage", ErrorDocumentId.ToString());
                x.SetAttribute("simple", Simple.ToString());
                Save(x.OwnerDocument);
                ClearCheckPages();
            }

            new Access().FireAfterAddProtection(new Document(DocumentId), e);
        }

        public static void RemoveProtection(int DocumentId)
        {
            //event
            var e = new RemoveProtectionEventArgs();
            new Access().FireBeforeRemoveProtection(new Document(DocumentId), e);

            if (e.Cancel) return;

            using (new WriteLock(Locker))
            {
                var x = (XmlElement)GetPage(DocumentId);
                if (x == null) return;

                x.ParentNode.RemoveChild(x);
                Save(x.OwnerDocument);
                ClearCheckPages();
            }

            new Access().FireAfterRemoveProtection(new Document(DocumentId), e);
        } 
        #endregion

        #region Reading methods

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static bool IsProtectedByGroup(int DocumentId, int GroupId)
        {
            bool isProtected = false;

            var d = new Document(DocumentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(DocumentId, d.Path))
                {
                    var currentNode = GetPage(GetProtectedPage(d.Path));
                    if (currentNode.SelectSingleNode("./group [@id=" + GroupId + "]") != null)
                    {
                        isProtected = true;
                    }
                }
            }

            return isProtected;
        }

        public static bool IsProtectedByMembershipRole(int documentId, string role)
        {
            bool isProtected = false;

            var d = new CMSNode(documentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(documentId, d.Path))
                {
                    var currentNode = GetPage(GetProtectedPage(d.Path));
                    if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null)
                    {
                        isProtected = true;
                    }
                }
            }

            return isProtected;
        }

        public static string[] GetAccessingMembershipRoles(int documentId, string path)
        {
            var roles = new ArrayList();

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(documentId, path) == false)
                    return null;

                var currentNode = GetPage(GetProtectedPage(path));
                foreach (XmlNode n in currentNode.SelectNodes("./group"))
                {
                    roles.Add(n.Attributes.GetNamedItem("id").Value);
                }
            }

            return (string[])roles.ToArray(typeof(string));
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static member.MemberGroup[] GetAccessingGroups(int DocumentId)
        {
            var d = new Document(DocumentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(DocumentId, d.Path) == false)
                    return null;

                var currentNode = GetPage(GetProtectedPage(d.Path));
                var mg = new member.MemberGroup[currentNode.SelectNodes("./group").Count];
                int count = 0;
                foreach (XmlNode n in currentNode.SelectNodes("./group"))
                {
                    mg[count] = new member.MemberGroup(int.Parse(n.Attributes.GetNamedItem("id").Value));
                    count++;
                }
                return mg;
            }

        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static member.Member GetAccessingMember(int DocumentId)
        {
            var d = new Document(DocumentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(DocumentId, d.Path) == false)
                    return null;

                if (GetProtectionTypeInternal(DocumentId) != ProtectionType.Simple)
                    throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");

                var currentNode = GetPage(GetProtectedPage(d.Path));
                if (currentNode.Attributes.GetNamedItem("memberId") != null)
                    return new member.Member(int.Parse(
                        currentNode.Attributes.GetNamedItem("memberId").Value));
            }

            throw new Exception("Document doesn't contain a memberId. This might be caused if document is protected using umbraco RC1 or older.");
        }

        public static MembershipUser GetAccessingMembershipUser(int documentId)
        {
            var d = new CMSNode(documentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(documentId, d.Path) == false)
                    return null;

                if (GetProtectionTypeInternal(documentId) != ProtectionType.Simple)
                    throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");

                var currentNode = GetPage(GetProtectedPage(d.Path));
                if (currentNode.Attributes.GetNamedItem("memberId") != null)
                {
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                    return provider.GetUser(currentNode.Attributes.GetNamedItem("memberId").Value, false);
                }
            }

            throw new Exception("Document doesn't contain a memberId. This might be caused if document is protected using umbraco RC1 or older.");
        }


        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static bool HasAccess(int DocumentId, member.Member Member)
        {
            bool hasAccess = false;

            var d = new Document(DocumentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(DocumentId, d.Path) == false)
                {
                    hasAccess = true;
                }
                else
                {
                    var currentNode = GetPage(GetProtectedPage(d.Path));
                    if (Member != null)
                    {
                        var ide = Member.Groups.GetEnumerator();
                        while (ide.MoveNext())
                        {
                            var mg = (member.MemberGroup)ide.Value;
                            if (currentNode.SelectSingleNode("./group [@id=" + mg.Id.ToString() + "]") != null)
                            {
                                hasAccess = true;
                                break;
                            }
                        }
                    }
                }
            }

            return hasAccess;
        }

        [Obsolete("This method has been replaced because of a spelling mistake. Use the HasAccess method instead.", false)]
        public static bool HasAccces(int documentId, object memberId)
        {
            // Call the correctly named version of this method
            return HasAccess(documentId, memberId);
        }

        public static bool HasAccess(int documentId, object memberId)
        {
            bool hasAccess = false;
            var node = new CMSNode(documentId);

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(documentId, node.Path) == false)
                    return true;

                var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                var member = provider.GetUser(memberId, false);
                var currentNode = GetPage(GetProtectedPage(node.Path));

                if (member != null)
                {
                    foreach (string role in Roles.GetRolesForUser(member.UserName))
                    {
                        if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null)
                        {
                            hasAccess = true;
                            break;
                        }
                    }
                }
            }

            return hasAccess;
        }

        public static bool HasAccess(int documentId, string path, MembershipUser member)
        {
            bool hasAccess = false;

            using (new ReadLock(Locker))
            {
                if (IsProtectedInternal(documentId, path) == false)
                {
                    hasAccess = true;
                }
                else
                {
                    XmlNode currentNode = GetPage(GetProtectedPage(path));
                    if (member != null)
                    {
                        string[] roles = Roles.GetRolesForUser(member.UserName);
                        foreach (string role in roles)
                        {
                            if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null)
                            {
                                hasAccess = true;
                                break;
                            }
                        }
                    }
                }
            }

            return hasAccess;
        }

        public static ProtectionType GetProtectionType(int DocumentId)
        {
            using (new ReadLock(Locker))
            {
                XmlNode x = GetPage(DocumentId);
                try
                {
                    return bool.Parse(x.Attributes.GetNamedItem("simple").Value)
                        ? ProtectionType.Simple
                        : ProtectionType.Advanced;
                }
                catch
                {
                    return ProtectionType.NotProtected;
                }
            }

        }

        public static bool IsProtected(int DocumentId, string Path)
        {
            using (new ReadLock(Locker))
            {
                return IsProtectedInternal(DocumentId, Path);
            }
        }

        public static int GetErrorPage(string Path)
        {
            using (new ReadLock(Locker))
            {
                return int.Parse(GetPage(GetProtectedPage(Path)).Attributes.GetNamedItem("noRightsPage").Value);
            }
        }

        public static int GetLoginPage(string Path)
        {
            using (new ReadLock(Locker))
            {
                return int.Parse(GetPage(GetProtectedPage(Path)).Attributes.GetNamedItem("loginPage").Value);
            }
        } 
        #endregion

        private static ProtectionType GetProtectionTypeInternal(int DocumentId)
        {
            //NOTE: No locks here! the locking is done in callers to this method

            XmlNode x = GetPage(DocumentId);
            try
            {
                return bool.Parse(x.Attributes.GetNamedItem("simple").Value)
                    ? ProtectionType.Simple
                    : ProtectionType.Advanced;
            }
            catch
            {
                return ProtectionType.NotProtected;
            }
        }

        private static bool IsProtectedInternal(int DocumentId, string Path)
        {
            //NOTE: No locks here! the locking is done in callers to this method

            bool isProtected = false;

            if (CheckedPages.ContainsKey(DocumentId) == false)
            {
                foreach (string id in Path.Split(','))
                {
                    if (GetPage(int.Parse(id)) != null)
                    {
                        isProtected = true;
                        break;
                    }
                }

                if (CheckedPages.ContainsKey(DocumentId) == false)
                {
                    CheckedPages.Add(DocumentId, isProtected);
                }
            }
            else
            {
                isProtected = (bool)CheckedPages[DocumentId];
            }

            return isProtected;
        }

        private static void Save(XmlDocument newDoc)
        {
            //NOTE: No locks here! the locking is done in callers to this method

            var e = new SaveEventArgs();

            new Access().FireBeforeSave(e);

            if (e.Cancel) return;

            using (var f = File.Open(_accessXmlFilePath, FileMode.Create))
            {
                newDoc.Save(f);
                f.Close();
                //set the underlying in-mem object to null so it gets re-read
                _accessXmlContent = null;
            }

            new Access().FireAfterSave(e);
        }

        private static int GetProtectedPage(string Path)
        {
            //NOTE: No locks here! the locking is done in callers to this method

            int protectedPage = 0;

            foreach (string id in Path.Split(','))
                if (GetPage(int.Parse(id)) != null)
                    protectedPage = int.Parse(id);

            return protectedPage;
        }

        private static XmlNode GetPage(int documentId)
        {
            //NOTE: No locks here! the locking is done in callers to this method
            var xDoc = GetXmlDocumentCopy();

            var x = xDoc.SelectSingleNode("/access/page [@id=" + documentId + "]");
            return x;
        }

        private static void ClearCheckPages()
        {
            CheckedPages.Clear();
        }

        //Event delegates
        public delegate void SaveEventHandler(Access sender, SaveEventArgs e);

        public delegate void AddProtectionEventHandler(Document sender, AddProtectionEventArgs e);
        public delegate void RemoveProtectionEventHandler(Document sender, RemoveProtectionEventArgs e);

        public delegate void AddMemberShipRoleToDocumentEventHandler(Document sender, string role, AddMemberShipRoleToDocumentEventArgs e);
        public delegate void RemoveMemberShipRoleFromDocumentEventHandler(Document sender, string role, RemoveMemberShipRoleFromDocumentEventArgs e);

        public delegate void RemoveMemberShipUserFromDocumentEventHandler(Document sender, string MembershipUserName, RemoveMemberShipUserFromDocumentEventArgs e);
        public delegate void AddMembershipUserToDocumentEventHandler(Document sender, string MembershipUserName, AddMembershipUserToDocumentEventArgs e);

        //Events

        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event AddProtectionEventHandler BeforeAddProtection;
        protected virtual void FireBeforeAddProtection(Document doc, AddProtectionEventArgs e)
        {
            if (BeforeAddProtection != null)
                BeforeAddProtection(doc, e);
        }

        public static event AddProtectionEventHandler AfterAddProtection;
        protected virtual void FireAfterAddProtection(Document doc, AddProtectionEventArgs e)
        {
            if (AfterAddProtection != null)
                AfterAddProtection(doc, e);
        }

        public static event RemoveProtectionEventHandler BeforeRemoveProtection;
        protected virtual void FireBeforeRemoveProtection(Document doc, RemoveProtectionEventArgs e)
        {
            if (BeforeRemoveProtection != null)
                BeforeRemoveProtection(doc, e);
        }

        public static event RemoveProtectionEventHandler AfterRemoveProtection;
        protected virtual void FireAfterRemoveProtection(Document doc, RemoveProtectionEventArgs e)
        {
            if (AfterRemoveProtection != null)
                AfterRemoveProtection(doc, e);
        }

        public static event AddMemberShipRoleToDocumentEventHandler BeforeAddMemberShipRoleToDocument;
        protected virtual void FireBeforeAddMemberShipRoleToDocument(Document doc, string role, AddMemberShipRoleToDocumentEventArgs e)
        {
            if (BeforeAddMemberShipRoleToDocument != null)
                BeforeAddMemberShipRoleToDocument(doc, role, e);
        }

        public static event AddMemberShipRoleToDocumentEventHandler AfterAddMemberShipRoleToDocument;
        protected virtual void FireAfterAddMemberShipRoleToDocument(Document doc, string role, AddMemberShipRoleToDocumentEventArgs e)
        {
            if (AfterAddMemberShipRoleToDocument != null)
                AfterAddMemberShipRoleToDocument(doc, role, e);
        }

        public static event RemoveMemberShipRoleFromDocumentEventHandler BeforeRemoveMemberShipRoleToDocument;
        protected virtual void FireBeforeRemoveMemberShipRoleFromDocument(Document doc, string role, RemoveMemberShipRoleFromDocumentEventArgs e)
        {
            if (BeforeRemoveMemberShipRoleToDocument != null)
                BeforeRemoveMemberShipRoleToDocument(doc, role, e);
        }

        public static event RemoveMemberShipRoleFromDocumentEventHandler AfterRemoveMemberShipRoleToDocument;
        protected virtual void FireAfterRemoveMemberShipRoleFromDocument(Document doc, string role, RemoveMemberShipRoleFromDocumentEventArgs e)
        {
            if (AfterRemoveMemberShipRoleToDocument != null)
                AfterRemoveMemberShipRoleToDocument(doc, role, e);
        }

        public static event RemoveMemberShipUserFromDocumentEventHandler BeforeRemoveMembershipUserFromDocument;
        protected virtual void FireBeforeRemoveMembershipUserFromDocument(Document doc, string username, RemoveMemberShipUserFromDocumentEventArgs e)
        {
            if (BeforeRemoveMembershipUserFromDocument != null)
                BeforeRemoveMembershipUserFromDocument(doc, username, e);
        }

        public static event RemoveMemberShipUserFromDocumentEventHandler AfterRemoveMembershipUserFromDocument;
        protected virtual void FireAfterRemoveMembershipUserFromDocument(Document doc, string username, RemoveMemberShipUserFromDocumentEventArgs e)
        {
            if (AfterRemoveMembershipUserFromDocument != null)
                AfterRemoveMembershipUserFromDocument(doc, username, e);
        }

        public static event AddMembershipUserToDocumentEventHandler BeforeAddMembershipUserToDocument;
        protected virtual void FireBeforeAddMembershipUserToDocument(Document doc, string username, AddMembershipUserToDocumentEventArgs e)
        {
            if (BeforeAddMembershipUserToDocument != null)
                BeforeAddMembershipUserToDocument(doc, username, e);
        }

        public static event AddMembershipUserToDocumentEventHandler AfterAddMembershipUserToDocument;
        protected virtual void FireAfterAddMembershipUserToDocument(Document doc, string username, AddMembershipUserToDocumentEventArgs e)
        {
            if (AfterAddMembershipUserToDocument != null)
                AfterAddMembershipUserToDocument(doc, username, e);
        }
    }

    public enum ProtectionType
    {
        NotProtected,
        Simple,
        Advanced
    }
}
