using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Used to ensure that the public access data file is kept up to date properly
    /// </summary>
    public sealed class PublicAccessEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            MemberGroupService.Saved += MemberGroupService_Saved;
        }

        static void MemberGroupService_Saved(IMemberGroupService sender, Core.Events.SaveEventArgs<Core.Models.IMemberGroup> e)
        {
            foreach (var grp in e.SavedEntities)
            {
                //check if the name has changed
                if (grp.AdditionalData.ContainsKey("previousName") 
                    && grp.AdditionalData["previousName"] != null
                    && grp.AdditionalData["previousName"].ToString().IsNullOrWhiteSpace() == false
                    && grp.AdditionalData["previousName"].ToString() != grp.Name)
                {
                    ApplicationContext.Current.Services.PublicAccessService.RenameMemberGroupRoleRules(grp.AdditionalData["previousName"].ToString(), grp.Name);
                }
            }
        }
    }
}
