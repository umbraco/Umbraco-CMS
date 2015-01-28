using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for Access.
    /// </summary>
    [Obsolete("Use Umbraco.Core.Service.IPublicAccessService instead")]
    public class Access
    {

        [Obsolete("Do not access this property directly, it is not thread safe, use GetXmlDocumentCopy instead")]
        public static XmlDocument AccessXml
        {
            get { return GetXmlDocumentCopy(); }
        }

        //NOTE: This is here purely for backwards compat
        [Obsolete("This should never be used, the data is stored in the database now")]
        public static XmlDocument GetXmlDocumentCopy()
        {
            var allAccessEntries = ApplicationContext.Current.Services.PublicAccessService.GetAll().ToArray();

            var xml = XDocument.Parse("<access/>");
            foreach (var entry in allAccessEntries)
            {
                var pageXml = new XElement("page",
                    new XAttribute("id", entry.ProtectedNodeId),
                    new XAttribute("loginPage", entry.LoginNodeId),
                    new XAttribute("noRightsPage", entry.NoAccessNodeId));

                foreach (var rule in entry.Rules)
                {
                    if (rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
                    {
                        //if there is a member id claim then it is 'simple' (this is how legacy worked)
                        pageXml.Add(new XAttribute("simple", "True"));
                        pageXml.Add(new XAttribute("memberId", rule.RuleValue));
                    }
                    else if (rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
                    {
                        pageXml.Add(new XElement("group", new XAttribute("id", rule.RuleValue)));
                    }
                }

                xml.Root.Add(pageXml);
            }

            return xml.ToXmlDocument();
        }

        #region Manipulation methods

        public static void AddMembershipRoleToDocument(int documentId, string role)
        {
            //event
            var doc = new Document(documentId);
            var e = new AddMemberShipRoleToDocumentEventArgs();
            new Access().FireBeforeAddMemberShipRoleToDocument(doc, role, e);

            if (e.Cancel) return;


            var entry = ApplicationContext.Current.Services.PublicAccessService.AddOrUpdateRule(
                doc.ContentEntity,
                Constants.Conventions.PublicAccess.MemberRoleRuleType,
                role);

            if (entry == null)
            {
                throw new Exception("Document is not protected!");
            }

            Save();

            new Access().FireAfterAddMemberShipRoleToDocument(doc, role, e);
        }
        

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void AddMemberGroupToDocument(int DocumentId, int MemberGroupId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);

            if (content == null)
                throw new Exception("No content found with document id " + DocumentId);

            var entry = ApplicationContext.Current.Services.PublicAccessService.AddOrUpdateRule(
                content, 
                Constants.Conventions.PublicAccess.MemberGroupIdRuleType, 
                MemberGroupId.ToString(CultureInfo.InvariantCulture));

            Save();
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void AddMemberToDocument(int DocumentId, int MemberId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);

            if (content == null)
                throw new Exception("No content found with document id " + DocumentId);

            ApplicationContext.Current.Services.PublicAccessService.AddOrUpdateRule(
                content, 
                Constants.Conventions.PublicAccess.MemberIdRuleType, 
                MemberId.ToString(CultureInfo.InvariantCulture));

            Save();
        }

        public static void AddMembershipUserToDocument(int documentId, string membershipUserName)
        {
            //event
            var doc = new Document(documentId);
            var e = new AddMembershipUserToDocumentEventArgs();
            new Access().FireBeforeAddMembershipUserToDocument(doc, membershipUserName, e);

            if (e.Cancel) return;

            var entry = ApplicationContext.Current.Services.PublicAccessService.AddOrUpdateRule(
                doc.ContentEntity, 
                Constants.Conventions.PublicAccess.MemberUsernameRuleType, 
                membershipUserName);

            if (entry == null)
            {
                throw new Exception("Document is not protected!");
            }

            Save();
   
            new Access().FireAfterAddMembershipUserToDocument(doc, membershipUserName, e);
        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static void RemoveMemberGroupFromDocument(int DocumentId, int MemberGroupId)
        {
            var doc = new Document(DocumentId);

            var entry = ApplicationContext.Current.Services.PublicAccessService.AddOrUpdateRule(
                doc.ContentEntity, 
                Constants.Conventions.PublicAccess.MemberGroupIdRuleType, 
                MemberGroupId.ToString(CultureInfo.InvariantCulture));

            if (entry == null)
            {
                throw new Exception("Document is not protected!");
            }
            Save();
        }

        public static void RemoveMembershipRoleFromDocument(int documentId, string role)
        {
            var doc = new Document(documentId);
            var e = new RemoveMemberShipRoleFromDocumentEventArgs();
            new Access().FireBeforeRemoveMemberShipRoleFromDocument(doc, role, e);

            if (e.Cancel) return;

            ApplicationContext.Current.Services.PublicAccessService.RemoveRule(
                doc.ContentEntity,
                Constants.Conventions.PublicAccess.MemberRoleRuleType,
                role);

            Save();

            new Access().FireAfterRemoveMemberShipRoleFromDocument(doc, role, e);
        }

        public static bool RenameMemberShipRole(string oldRolename, string newRolename)
        {
            var hasChange = ApplicationContext.Current.Services.PublicAccessService.RenameMemberGroupRoleRules(oldRolename, newRolename);
            
            if (hasChange)
                Save();

            return hasChange;
        }

        public static void ProtectPage(bool Simple, int DocumentId, int LoginDocumentId, int ErrorDocumentId)
        {
            var doc = new Document(DocumentId);
            var e = new AddProtectionEventArgs();
            new Access().FireBeforeAddProtection(doc, e);

            if (e.Cancel) return;

            var loginContent = ApplicationContext.Current.Services.ContentService.GetById(LoginDocumentId);
            if (loginContent == null) throw new NullReferenceException("No content item found with id " + LoginDocumentId);
            var noAccessContent = ApplicationContext.Current.Services.ContentService.GetById(ErrorDocumentId);
            if (noAccessContent == null) throw new NullReferenceException("No content item found with id " + ErrorDocumentId);

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(doc.ContentEntity);
            if (entry != null)
            {
                if (Simple)
                {
                    // if using simple mode, make sure that all existing groups are removed
                    entry.ClearRules();    
                }
                
                //ensure the correct ids are applied
                entry.LoginNodeId = loginContent.Id;
                entry.NoAccessNodeId = noAccessContent.Id;
            }
            else
            {
                entry = new PublicAccessEntry(doc.ContentEntity, 
                    ApplicationContext.Current.Services.ContentService.GetById(LoginDocumentId),
                    ApplicationContext.Current.Services.ContentService.GetById(ErrorDocumentId),
                    new List<PublicAccessRule>());
            }

            ApplicationContext.Current.Services.PublicAccessService.Save(entry);

            Save();

            new Access().FireAfterAddProtection(new Document(DocumentId), e);
        }

        public static void RemoveProtection(int DocumentId)
        {
            //event
            var doc = new Document(DocumentId);
            var e = new RemoveProtectionEventArgs();
            new Access().FireBeforeRemoveProtection(doc, e);

            if (e.Cancel) return;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(doc.ContentEntity);
            if (entry != null)
            {
                ApplicationContext.Current.Services.PublicAccessService.Delete(entry);
            }

            Save();

            new Access().FireAfterRemoveProtection(doc, e);
        } 
        #endregion

        #region Reading methods

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static bool IsProtectedByGroup(int DocumentId, int GroupId)
        {
            var d = new Document(DocumentId);

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(d.ContentEntity);
            if (entry == null) return false;

            return entry.Rules
                .Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberGroupIdRuleType 
                    && x.RuleValue == GroupId.ToString(CultureInfo.InvariantCulture));

        }

        public static bool IsProtectedByMembershipRole(int documentId, string role)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(documentId);

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return false;

            return entry.Rules
                .Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType
                    && x.RuleValue == role);

        }

        public static string[] GetAccessingMembershipRoles(int documentId, string path)
        {
            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(path.EnsureEndsWith("," + documentId));
            if (entry == null) return new string[] { };

            var memberGroupRoleRules = entry.Rules.Where(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType);
            return memberGroupRoleRules.Select(x => x.RuleValue).ToArray();

        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static member.MemberGroup[] GetAccessingGroups(int DocumentId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);
            if (content == null) return null;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return null;

            var memberGroupIdRules = entry.Rules.Where(x => x.RuleType == Constants.Conventions.PublicAccess.MemberGroupIdRuleType);

            return memberGroupIdRules.Select(x => new member.MemberGroup(int.Parse(x.RuleValue))).ToArray();

        }

        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static member.Member GetAccessingMember(int DocumentId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);
            if (content == null) return null;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return null;

            //legacy would throw an exception here if it was not 'simple' and simple means based on a member id in this case
            if (entry.Rules.All(x => x.RuleType != Constants.Conventions.PublicAccess.MemberIdRuleType))
            {
                throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");
            }
            
            var memberIdRule = entry.Rules.First(x => x.RuleType == Constants.Conventions.PublicAccess.MemberIdRuleType);
            return new member.Member(int.Parse(memberIdRule.RuleValue));

        }

        public static MembershipUser GetAccessingMembershipUser(int documentId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(documentId);
            if (content == null) return null;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return null;

            //legacy would throw an exception here if it was not 'simple' and simple means based on a username
            if (entry.Rules.All(x => x.RuleType != Constants.Conventions.PublicAccess.MemberUsernameRuleType))
            {                
                throw new Exception("Document isn't protected using Simple mechanism. Use GetAccessingMemberGroups instead");
            }

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            var usernameRule = entry.Rules.First(x => x.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType);
            return provider.GetUser(usernameRule.RuleValue, false);

        }


        [Obsolete("This method is no longer supported. Use the ASP.NET MemberShip methods instead", true)]
        public static bool HasAccess(int DocumentId, member.Member Member)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);
            if (content == null) return true;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return true;

            var memberGroupIds = Member.Groups.Values.Cast<MemberGroup>().Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToArray();
            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberGroupIdRuleType
                                        && memberGroupIds.Contains(x.RuleValue));

        }

        [Obsolete("This method has been replaced because of a spelling mistake. Use the HasAccess method instead.", false)]
        public static bool HasAccces(int documentId, object memberId)
        {
            // Call the correctly named version of this method
            return HasAccess(documentId, memberId);
        }

        public static bool HasAccess(int documentId, object memberId)
        {
            return ApplicationContext.Current.Services.PublicAccessService.HasAccess(
                documentId, 
                memberId, 
                ApplicationContext.Current.Services.ContentService,
                MembershipProviderExtensions.GetMembersMembershipProvider(),
                //TODO: This should really be targeting a specific provider by name!!
                Roles.Provider);
        }

        public static bool HasAccess(int documentId, string path, MembershipUser member)
        {
            return ApplicationContext.Current.Services.PublicAccessService.HasAccess(
                 path,
                 member,
                //TODO: This should really be targeting a specific provider by name!!
                 Roles.Provider);
        }

        public static ProtectionType GetProtectionType(int DocumentId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(DocumentId);
            if (content == null) return ProtectionType.NotProtected;

            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null) return ProtectionType.NotProtected;

            //legacy states that if it is protected by a member id then it is 'simple'
            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberIdRuleType)
                ? ProtectionType.Simple 
                : ProtectionType.Advanced;

        }

        public static bool IsProtected(int DocumentId, string Path)
        {
            return ApplicationContext.Current.Services.PublicAccessService.IsProtected(Path.EnsureEndsWith("," + DocumentId));             
        }

        public static int GetErrorPage(string Path)
        {
            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(Path);
            if (entry == null) return -1;
            var entity = ApplicationContext.Current.Services.EntityService.Get(entry.NoAccessNodeId, UmbracoObjectTypes.Document, false);
            return entity.Id;

        }

        public static int GetLoginPage(string Path)
        {
            var entry = ApplicationContext.Current.Services.PublicAccessService.GetEntryForContent(Path);
            if (entry == null) return -1;
            var entity = ApplicationContext.Current.Services.EntityService.Get(entry.LoginNodeId, UmbracoObjectTypes.Document, false);
            return entity.Id;

        } 
        #endregion


        //NOTE: This is purely here for backwards compat for events
        private static void Save()
        {
            var e = new SaveEventArgs();

            new Access().FireBeforeSave(e);

            if (e.Cancel) return;

            new Access().FireAfterSave(e);
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
