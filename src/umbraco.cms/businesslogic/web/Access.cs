using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.IO;

using System.Web.Security;
using umbraco.IO;

namespace umbraco.cms.businesslogic.web
{
	/// <summary>
	/// Summary description for Access.
	/// </summary>
	public class Access
	{
		public Access()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		static private Hashtable _checkedPages = new Hashtable();

		//must be volatile for double check lock to work
		static private volatile XmlDocument _accessXmlContent;
		static private string _accessXmlSource;

		private static void clearCheckPages() 
		{
			_checkedPages.Clear();
		}

        static object _locko = new object();

		public static XmlDocument AccessXml 
		{
            get
            {
                if (_accessXmlContent == null)
                {
                    lock (_locko)
                    {
                        if (_accessXmlContent == null)
                        {
                            if (_accessXmlSource == null)
                            {
                                //if we pop it here it'll make for better stack traces ;)
                                _accessXmlSource = IOHelper.MapPath(SystemFiles.AccessXml, false);
                            }

                            _accessXmlContent = new XmlDocument();

                            if (!System.IO.File.Exists(_accessXmlSource))
                            {
                            	var file = new FileInfo(_accessXmlSource);
								if (!Directory.Exists(file.DirectoryName))
								{
									Directory.CreateDirectory(file.Directory.FullName); //ensure the folder exists!	
								}                            	
                                System.IO.FileStream f = System.IO.File.Open(_accessXmlSource, FileMode.Create);
                                System.IO.StreamWriter sw = new StreamWriter(f);
                                sw.WriteLine("<access/>");
                                sw.Close();
                                f.Close();
                            }
                            _accessXmlContent.Load(_accessXmlSource);
                        }
                    }
                }
                return _accessXmlContent;
            }
		}

		public static void AddMembershipRoleToDocument(int documentId, string role) {
			//event
			AddMemberShipRoleToDocumentEventArgs e = new AddMemberShipRoleToDocumentEventArgs();
			new Access().FireBeforeAddMemberShipRoleToDocument(new Document(documentId), role, e);

			if (!e.Cancel) {
				XmlElement x = (XmlElement)getPage(documentId);

				if (x == null)
					throw new Exception("Document is not protected!");
				else {
					if (x.SelectSingleNode("group [@id = '" + role + "']") == null) {
						XmlElement groupXml = (XmlElement)AccessXml.CreateNode(XmlNodeType.Element, "group", "");
						groupXml.SetAttribute("id", role);
						x.AppendChild(groupXml);
						save();
					}
				}

				new Access().FireAfterAddMemberShipRoleToDocument(new Document(documentId), role, e);
			}
		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static void AddMemberGroupToDocument(int DocumentId, int MemberGroupId) 
		{
			XmlElement x = (XmlElement) getPage(DocumentId);
			
			if (x == null)
				throw new Exception("Document is not protected!");
			else 
			{
				if (x.SelectSingleNode("group [@id = '" + MemberGroupId.ToString() + "']") == null) 
				{
					XmlElement groupXml = (XmlElement) AccessXml.CreateNode(XmlNodeType.Element, "group", "");
					groupXml.SetAttribute("id", MemberGroupId.ToString());
					x.AppendChild(groupXml);
					save();
				}
			}
		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static void AddMemberToDocument(int DocumentId, int MemberId) 
		{
			XmlElement x = (XmlElement) getPage(DocumentId);
			
			if (x == null)
				throw new Exception("Document is not protected!");
			else 
			{
				if (x.Attributes.GetNamedItem("memberId") != null)
					x.Attributes.GetNamedItem("memberId").Value = MemberId.ToString();
				else
					x.SetAttribute("memberId", MemberId.ToString());
				save();
			}
		}
		
		public static void AddMembershipUserToDocument(int documentId, string membershipUserName) {
			//event
			AddMembershipUserToDocumentEventArgs e = new AddMembershipUserToDocumentEventArgs();
			new Access().FireBeforeAddMembershipUserToDocument(new Document(documentId), membershipUserName, e);

			if (!e.Cancel) {
				XmlElement x = (XmlElement)getPage(documentId);

				if (x == null)
					throw new Exception("Document is not protected!");
				else {
					if (x.Attributes.GetNamedItem("memberId") != null)
						x.Attributes.GetNamedItem("memberId").Value = membershipUserName;
					else
						x.SetAttribute("memberId", membershipUserName);
					save();
				}

				new Access().FireAfterAddMembershipUserToDocument(new Document(documentId), membershipUserName, e);
			}
			
		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static void RemoveMemberGroupFromDocument(int DocumentId, int MemberGroupId) 
		{
			XmlElement x = (XmlElement) getPage(DocumentId);
			
			if (x == null)
				throw new Exception("Document is not protected!");
			else 
			{
				XmlNode xGroup = x.SelectSingleNode("group [@id = '" + MemberGroupId.ToString() + "']");
				if (xGroup != null) 
				{
					x.RemoveChild(xGroup);
					save();
				}
			}
		}

		public static void RemoveMembershipRoleFromDocument(int documentId, string role) {

			RemoveMemberShipRoleFromDocumentEventArgs e = new RemoveMemberShipRoleFromDocumentEventArgs();
			new Access().FireBeforeRemoveMemberShipRoleFromDocument(new Document(documentId), role, e);

			if (!e.Cancel) {
				XmlElement x = (XmlElement)getPage(documentId);

				if (x == null)
					throw new Exception("Document is not protected!");
				else {
					XmlNode xGroup = x.SelectSingleNode("group [@id = '" + role + "']");
					if (xGroup != null) {
						x.RemoveChild(xGroup);
						save();
					}
				}

				new Access().FireAfterRemoveMemberShipRoleFromDocument(new Document(documentId), role, e);
			}
		}

		public static bool RenameMemberShipRole(string oldRolename, string newRolename)
		{
			bool hasChange = false;
			if (oldRolename != newRolename)
			{
				foreach (XmlNode x in AccessXml.SelectNodes("//group [@id = '" + oldRolename + "']"))
				{
					x.Attributes["id"].Value = newRolename;
					hasChange = true;
				}
				if (hasChange)
					save();
			}

			return hasChange;
	
		}
		
		public static void ProtectPage(bool Simple, int DocumentId, int LoginDocumentId, int ErrorDocumentId) 
		{
			AddProtectionEventArgs e = new AddProtectionEventArgs();
			new Access().FireBeforeAddProtection(new Document(DocumentId), e);

			if (!e.Cancel) {

				XmlElement x = (XmlElement)getPage(DocumentId);
				if (x == null) {
					x = (XmlElement)_accessXmlContent.CreateNode(XmlNodeType.Element, "page", "");
					AccessXml.DocumentElement.AppendChild(x);
				}
					// if using simple mode, make sure that all existing groups are removed
				else if (Simple) {
					x.RemoveAll();
				}
				x.SetAttribute("id", DocumentId.ToString());
				x.SetAttribute("loginPage", LoginDocumentId.ToString());
				x.SetAttribute("noRightsPage", ErrorDocumentId.ToString());
				x.SetAttribute("simple", Simple.ToString());
				save();

				clearCheckPages();

				new Access().FireAfterAddProtection(new Document(DocumentId), e);
			}
		}

		public static void RemoveProtection(int DocumentId) 
		{
			XmlElement x = (XmlElement) getPage(DocumentId);
			if (x != null) 
			{
				//event
				RemoveProtectionEventArgs e = new RemoveProtectionEventArgs();
				new Access().FireBeforeRemoveProtection(new Document(DocumentId), e);

				if (!e.Cancel) {

					x.ParentNode.RemoveChild(x);
					save();
					clearCheckPages();

					new Access().FireAfterRemoveProtection(new Document(DocumentId), e);
				}

			}
		}

		private static void save() 
		{
			SaveEventArgs e = new SaveEventArgs();

			new Access().FireBeforeSave(e);

			if (!e.Cancel) {
				System.IO.FileStream f = System.IO.File.Open(_accessXmlSource, FileMode.Create);
				AccessXml.Save(f);
				f.Close();

				new Access().FireAfterSave(e);
			}
		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static bool IsProtectedByGroup(int DocumentId, int GroupId) 
		{
			bool isProtected = false;

			cms.businesslogic.web.Document d = new Document(DocumentId);

			if (!IsProtected(DocumentId, d.Path))
				isProtected = false;
			else 
			{
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				if (currentNode.SelectSingleNode("./group [@id=" + GroupId.ToString() + "]") != null) 
				{
					isProtected = true;
				}
			}

			return isProtected;
		}

		public static bool IsProtectedByMembershipRole(int documentId, string role) {
			bool isProtected = false;

			CMSNode d = new CMSNode(documentId);

			if (!IsProtected(documentId, d.Path))
				isProtected = false;
			else {
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null) {
					isProtected = true;
				}
			}

			return isProtected;
		}

		public static string[] GetAccessingMembershipRoles(int documentId, string path) {
			ArrayList roles = new ArrayList();

			if (!IsProtected(documentId, path))
				return null;
			else {
				XmlNode currentNode = getPage(getProtectedPage(path));
				foreach (XmlNode n in currentNode.SelectNodes("./group")) {
					roles.Add(n.Attributes.GetNamedItem("id").Value);
				}
				return (string[])roles.ToArray(typeof(string));
			}

		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static cms.businesslogic.member.MemberGroup[] GetAccessingGroups(int DocumentId) 
		{
			cms.businesslogic.web.Document d = new Document(DocumentId);

			if (!IsProtected(DocumentId, d.Path))
				return null;
			else 
			{
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				cms.businesslogic.member.MemberGroup[] mg = new umbraco.cms.businesslogic.member.MemberGroup[currentNode.SelectNodes("./group").Count];
				int count = 0;
				foreach (XmlNode n in currentNode.SelectNodes("./group"))
				{
					mg[count] = new cms.businesslogic.member.MemberGroup(int.Parse(n.Attributes.GetNamedItem("id").Value));
					count++;
				}
				return mg;
			}

		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static cms.businesslogic.member.Member GetAccessingMember(int DocumentId) {
			cms.businesslogic.web.Document d = new Document(DocumentId);

			if (!IsProtected(DocumentId, d.Path))
				return null;
			else if (GetProtectionType(DocumentId) != ProtectionType.Simple)
				throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");
			else {
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				if (currentNode.Attributes.GetNamedItem("memberId") != null)
					return new cms.businesslogic.member.Member(int.Parse(
						currentNode.Attributes.GetNamedItem("memberId").Value));
				else
					throw new Exception("Document doesn't contain a memberId. This might be caused if document is protected using umbraco RC1 or older.");

			}

		}
		
		public static MembershipUser GetAccessingMembershipUser(int documentId) {
			CMSNode d = new CMSNode(documentId);

			if (!IsProtected(documentId, d.Path))
				return null;
			else if (GetProtectionType(documentId) != ProtectionType.Simple)
				throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");
			else {
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				if (currentNode.Attributes.GetNamedItem("memberId") != null)
					return Membership.GetUser(currentNode.Attributes.GetNamedItem("memberId").Value);
				else
					throw new Exception("Document doesn't contain a memberId. This might be caused if document is protected using umbraco RC1 or older.");

			}

		}


		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		public static bool HasAccess(int DocumentId, cms.businesslogic.member.Member Member) 
		{
			bool hasAccess = false;

			cms.businesslogic.web.Document d = new Document(DocumentId);

			if (!IsProtected(DocumentId, d.Path))
				hasAccess = true;
			else 
			{
				XmlNode currentNode = getPage(getProtectedPage(d.Path));
				if (Member != null) 
				{
					IDictionaryEnumerator ide = Member.Groups.GetEnumerator();
					while(ide.MoveNext())
					{
						cms.businesslogic.member.MemberGroup mg = (cms.businesslogic.member.MemberGroup) ide.Value;
						if (currentNode.SelectSingleNode("./group [@id=" + mg.Id.ToString() + "]") != null) 
						{
							hasAccess = true;
							break;
						}
					}
				}
			}

			return hasAccess;
		}

		public static bool HasAccces(int documentId, object memberId) {
			bool hasAccess = false;
			cms.businesslogic.CMSNode node = new CMSNode(documentId);

			if (!IsProtected(documentId, node.Path))
				return true;
			else {
				MembershipUser member = Membership.GetUser(memberId);
				XmlNode currentNode = getPage(getProtectedPage(node.Path));

				if (member != null) {
					foreach(string role in Roles.GetRolesForUser()) {
						if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null) {
							hasAccess = true;
							break;
						}
					}
				}
			}
			return hasAccess;
		}

		[Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
		private static bool HasAccess(int DocumentId, string Path, cms.businesslogic.member.Member Member) 
		{
			bool hasAccess = false;

			if (!IsProtected(DocumentId, Path))
				hasAccess = true;
			else 
			{
				XmlNode currentNode = getPage(getProtectedPage(Path));
				if (Member != null) 
				{
					IDictionaryEnumerator ide = Member.Groups.GetEnumerator();
					while(ide.MoveNext())
					{
						cms.businesslogic.member.MemberGroup mg = (cms.businesslogic.member.MemberGroup) ide.Value;
						if (currentNode.SelectSingleNode("./group [@id=" + mg.Id.ToString() + "]") != null) 
						{
							hasAccess = true;
							break;
						}
					}
				}
			}

			return hasAccess;
		}

		public static bool HasAccess(int documentId, string path, MembershipUser member) {
			bool hasAccess = false;

			if (!IsProtected(documentId, path))
				hasAccess = true;
			else {
				XmlNode currentNode = getPage(getProtectedPage(path));
				if (member != null) {
					string[] roles = Roles.GetRolesForUser(member.UserName);
					foreach(string role in roles) {
						if (currentNode.SelectSingleNode("./group [@id='" + role + "']") != null) {
							hasAccess = true;
							break;
						}
					}
				}
			}

			return hasAccess;
		}

		public static ProtectionType GetProtectionType(int DocumentId) 
		{
			XmlNode x = getPage(DocumentId);
			try 
			{
				if (bool.Parse(x.Attributes.GetNamedItem("simple").Value))
					return ProtectionType.Simple;
				else
					return ProtectionType.Advanced;
			} 
			catch 
			{
				return ProtectionType.NotProtected;
			}

		}

		public static bool IsProtected(int DocumentId, string Path) 
		{
			bool isProtected = false;

			if (!_checkedPages.ContainsKey(DocumentId)) 
			{
				foreach(string id in Path.Split(',')) 
				{
					if (getPage(int.Parse(id)) != null) 
					{
						isProtected = true;
						break;
					}
				}

				// Add thread safe updating to the hashtable
                if (System.Web.HttpContext.Current != null)
    				System.Web.HttpContext.Current.Application.Lock();
				if (!_checkedPages.ContainsKey(DocumentId))
					_checkedPages.Add(DocumentId, isProtected);
                if (System.Web.HttpContext.Current != null)
                    System.Web.HttpContext.Current.Application.UnLock();
			}
			else
				isProtected = (bool) _checkedPages[DocumentId];
			
			return isProtected;
		}

		public static int GetErrorPage(string Path) 
		{
			return int.Parse(getPage(getProtectedPage(Path)).Attributes.GetNamedItem("noRightsPage").Value);
		}

		public static int GetLoginPage(string Path) 
		{
			return int.Parse(getPage(getProtectedPage(Path)).Attributes.GetNamedItem("loginPage").Value);
		}

		private static int getProtectedPage(string Path) 
		{
			int protectedPage = 0;

			foreach(string id in Path.Split(',')) 
				if (getPage(int.Parse(id)) != null) 
					protectedPage = int.Parse(id);

			return protectedPage;
		}

		private static XmlNode getPage(int documentId) 
		{
			XmlNode x = AccessXml.SelectSingleNode("/access/page [@id=" + documentId.ToString() + "]");
			return x;
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
		protected virtual void FireBeforeSave(SaveEventArgs e) {
			if (BeforeSave != null)
				BeforeSave(this, e);
		}

		public static event SaveEventHandler AfterSave;
		protected virtual void FireAfterSave(SaveEventArgs e) {
			if (AfterSave != null)
				AfterSave(this, e);
		}

		public static event AddProtectionEventHandler BeforeAddProtection;
		protected virtual void FireBeforeAddProtection(Document doc, AddProtectionEventArgs e) {
			if (BeforeAddProtection != null)
				BeforeAddProtection(doc, e);
		}

		public static event AddProtectionEventHandler AfterAddProtection;
		protected virtual void FireAfterAddProtection(Document doc, AddProtectionEventArgs e) {
			if (AfterAddProtection != null)
				AfterAddProtection(doc, e);
		}

		public static event RemoveProtectionEventHandler BeforeRemoveProtection;
		protected virtual void FireBeforeRemoveProtection(Document doc, RemoveProtectionEventArgs e) {
			if (BeforeRemoveProtection != null)
				BeforeRemoveProtection(doc, e);
		}

		public static event RemoveProtectionEventHandler AfterRemoveProtection;
		protected virtual void FireAfterRemoveProtection(Document doc, RemoveProtectionEventArgs e) {
			if (AfterRemoveProtection != null)
				AfterRemoveProtection(doc, e);
		}

		public static event AddMemberShipRoleToDocumentEventHandler BeforeAddMemberShipRoleToDocument;
		protected virtual void FireBeforeAddMemberShipRoleToDocument(Document doc, string role, AddMemberShipRoleToDocumentEventArgs e) {
			if (BeforeAddMemberShipRoleToDocument != null)
				BeforeAddMemberShipRoleToDocument(doc, role, e);
		}

		public static event AddMemberShipRoleToDocumentEventHandler AfterAddMemberShipRoleToDocument;
		protected virtual void FireAfterAddMemberShipRoleToDocument(Document doc, string role, AddMemberShipRoleToDocumentEventArgs e) {
			if (AfterAddMemberShipRoleToDocument != null)
				AfterAddMemberShipRoleToDocument(doc, role, e);
		}

		public static event RemoveMemberShipRoleFromDocumentEventHandler BeforeRemoveMemberShipRoleToDocument;
		protected virtual void FireBeforeRemoveMemberShipRoleFromDocument(Document doc, string role, RemoveMemberShipRoleFromDocumentEventArgs e) {
			if (BeforeRemoveMemberShipRoleToDocument != null)
				BeforeRemoveMemberShipRoleToDocument(doc, role, e);
		}

		public static event RemoveMemberShipRoleFromDocumentEventHandler AfterRemoveMemberShipRoleToDocument;
		protected virtual void FireAfterRemoveMemberShipRoleFromDocument(Document doc, string role, RemoveMemberShipRoleFromDocumentEventArgs e) {
			if (AfterRemoveMemberShipRoleToDocument != null)
				AfterRemoveMemberShipRoleToDocument(doc, role, e);
		}

		public static event RemoveMemberShipUserFromDocumentEventHandler BeforeRemoveMembershipUserFromDocument;
		protected virtual void FireBeforeRemoveMembershipUserFromDocument(Document doc, string username, RemoveMemberShipUserFromDocumentEventArgs e) {
			if (BeforeRemoveMembershipUserFromDocument != null)
				BeforeRemoveMembershipUserFromDocument(doc, username, e);
		}

		public static event RemoveMemberShipUserFromDocumentEventHandler AfterRemoveMembershipUserFromDocument;
		protected virtual void FireAfterRemoveMembershipUserFromDocument(Document doc, string username, RemoveMemberShipUserFromDocumentEventArgs e) {
			if (AfterRemoveMembershipUserFromDocument != null)
				AfterRemoveMembershipUserFromDocument(doc, username, e);
		}

		public static event AddMembershipUserToDocumentEventHandler BeforeAddMembershipUserToDocument;
		protected virtual void FireBeforeAddMembershipUserToDocument(Document doc, string username, AddMembershipUserToDocumentEventArgs e) {
			if (BeforeAddMembershipUserToDocument != null)
				BeforeAddMembershipUserToDocument(doc, username, e);
		}

		public static event AddMembershipUserToDocumentEventHandler AfterAddMembershipUserToDocument;
		protected virtual void FireAfterAddMembershipUserToDocument(Document doc, string username, AddMembershipUserToDocumentEventArgs e) {
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
