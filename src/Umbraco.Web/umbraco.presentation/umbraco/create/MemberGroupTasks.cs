using System.Linq;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Web;
using Umbraco.Web.UI;

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
            if (Member.IsUsingUmbracoRoles())
            {
                roleDeleted = Roles.DeleteRole(Alias);
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

        public override string ReturnUrl => _returnUrl;

        public override string AssignedApp => DefaultApps.member.ToString();
    }
}
