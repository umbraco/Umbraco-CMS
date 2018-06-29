using System.Linq;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class MemberGroupTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            Roles.CreateRole(Alias);
            _returnUrl = $"members/EditMemberGroup.aspx?id={System.Web.HttpContext.Current.Server.UrlEncode(Alias)}";
            return true;
        }

        public override bool PerformDelete()
        {
            var roleDeleted = false;

            // only built-in roles can be deleted
            if (Roles.Provider.Name == Constants.Conventions.Member.UmbracoRoleProviderName)
            {
                var group = Current.Services.MemberGroupService.GetByName(Alias);
                if (group != null)
                {
                    Current.Services.MemberGroupService.Delete(group);
                }
            }

            // Need to delete the member group from any content item that has it assigned in public access settings
            var publicAccessService = Current.Services.PublicAccessService;
            var allPublicAccessRules = publicAccessService.GetAll();

            // Find only rules which have the current role name (alias) assigned to them
            var rulesWithDeletedRoles = allPublicAccessRules.Where(x => x.Rules.Any(r => r.RuleValue == Alias));

            var contentService = Current.Services.ContentService;
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

        public override string ReturnUrl => _returnUrl;

        public override string AssignedApp => Constants.Applications.Members.ToString();
    }
}
