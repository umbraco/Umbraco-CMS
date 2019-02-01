using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with member groups
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MemberGroups)]
    public class MemberGroupController : UmbracoAuthorizedJsonController
    {
        private readonly MembershipProvider _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

        public MemberGroupDisplay GetById(int id)
        {
            var memberGroup = Services.MemberGroupService.GetById(id);
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);
            return dto;
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

        public MemberGroupDisplay PostSave(MemberGroupSave saveModel)
        {
            var service = Services.MemberGroupService;

            var id = int.Parse(saveModel.Id.ToString());
            var memberGroup = id > 0 ? service.GetById(id) : new MemberGroup();
            if (memberGroup == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            memberGroup.Name = saveModel.Name;
            service.Save(memberGroup);

            var display = Mapper.Map<IMemberGroup, MemberGroupDisplay>(memberGroup);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/memberGroupSavedHeader"),
                            string.Empty);

            return display;
        }
    }
}
