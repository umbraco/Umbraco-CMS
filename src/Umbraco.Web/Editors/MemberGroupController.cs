using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with member groups
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MemberGroups)]
    [MemberGroupControllerConfiguration]
    public class MemberGroupController : UmbracoAuthorizedJsonController
    {
        private readonly MembershipProvider _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class MemberGroupControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))
                ));
            }
        }

        /// <summary>
        /// Gets the member group json for the member group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MemberGroupDisplay GetById(int id)
        {
            var memberGroup = Services.MemberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        /// <summary>
        /// Gets the member group json for the member group guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MemberGroupDisplay GetById(Guid id)
        {
            var memberGroup = Services.MemberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        /// <summary>
        /// Gets the member group json for the member group udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MemberGroupDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var memberGroup = Services.MemberGroupService.GetById(guidUdi.Guid);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
        }

        public IEnumerable<MemberGroupDisplay> GetByIds([FromUri]int[] ids)
        {
            if (_provider.IsUmbracoMembershipProvider())
            {
                return Services.MemberGroupService.GetByIds(ids)
                    .Select(Mapper.Map<IMemberGroup, MemberGroupDisplay>);
            }

            return Enumerable.Empty<MemberGroupDisplay>();
        }

        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var memberGroup = Services.MemberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.MemberGroupService.Delete(memberGroup);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public IEnumerable<MemberGroupDisplay> GetAllGroups()
        {
            if (_provider.IsUmbracoMembershipProvider())
            {
                return Services.MemberGroupService.GetAll()
                    .Select(Mapper.Map<IMemberGroup, MemberGroupDisplay>);
            }

            return Enumerable.Empty<MemberGroupDisplay>();
        }

        public MemberGroupDisplay GetEmpty()
        {
            var item = new MemberGroup();
            return Mapper.Map<IMemberGroup, MemberGroupDisplay>(item);
        }

        public bool IsMemberGroupNameUnique(int id, string oldName, string newName)
        {
            if (newName == oldName)
                return true; // name hasn't changed

            var service = Services.MemberGroupService;
            var memberGroup = service.GetByName(newName);
            if (memberGroup == null)
                return true; // no member group found

            return memberGroup.Id == id;
        }

        public MemberGroupDisplay PostSave(MemberGroupSave saveModel)
        {
            var service = Services.MemberGroupService;

            var id = int.Parse(saveModel.Id.ToString());
            var memberGroup = id > 0 ? service.GetById(id) : new MemberGroup();
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (IsMemberGroupNameUnique(memberGroup.Id, memberGroup.Name, saveModel.Name))
            {
                memberGroup.Name = saveModel.Name;
                service.Save(memberGroup);

                var display = Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
                display.AddSuccessNotification(
                                Services.TextService.Localize("speechBubbles", "memberGroupSavedHeader"),
                                string.Empty);

                return display;
            }
            else
            {
                var display = Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
                display.AddErrorNotification(
                                Services.TextService.Localize("speechBubbles", "memberGroupNameDuplicate"),
                                string.Empty);

                return display;
            }            
        }
    }
}
