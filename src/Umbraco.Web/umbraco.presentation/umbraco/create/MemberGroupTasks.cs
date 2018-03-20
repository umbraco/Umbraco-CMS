using System;
using System.Data;
using System.Linq;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Web;

namespace umbraco
{
    public class MemberGroupTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            Roles.CreateRole(Alias);
            _returnUrl = string.Format("members/EditMemberGroup.aspx?id={0}", System.Web.HttpContext.Current.Server.UrlEncode(Alias));
            return true;
        }

        public override bool PerformDelete()
        {
            var roleDeleted = false;

            // only built-in roles can be deleted
            if (Member.IsUsingUmbracoRoles())
            {
                MemberGroup.GetByName(Alias).delete();
                roleDeleted = true;
            }

            // Need to delete the member group from any content item that has it assigned in public access settings
            var publicAccessService = UmbracoContext.Current.Application.Services.PublicAccessService;
            var allPublicAccessRules = publicAccessService.GetAll();

            // Find only rules which have the current role name (alias) assigned to them
            var rulesWithDeletedRoles = allPublicAccessRules.Where(x => x.Rules.Any(r => r.RuleValue == Alias));

            var contentService = UmbracoContext.Current.Application.Services.ContentService;
            foreach (var publicAccessEntry in rulesWithDeletedRoles)
            {
                var contentItem = contentService.GetById(publicAccessEntry.ProtectedNodeId);
                var rulesToDelete = publicAccessEntry.Rules.ToList();
                foreach (var rule in rulesToDelete)
                    publicAccessService.RemoveRule(contentItem, rule.RuleType, rule.RuleValue);
            }

            return roleDeleted;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.member.ToString(); }
        }
    }
}
