using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Examine;
using System.Web.Security;
using Member = umbraco.cms.businesslogic.member.Member;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the member application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Members)]
    public class MemberController : ContentControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MemberController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Gets the content json for the member
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public MemberDisplay GetByLogin(string loginName)
        {
            if (Member.InUmbracoMemberMode())
            {
                var foundMember = Services.MemberService.GetByUsername(loginName);
                if (foundMember == null)
                {
                    HandleContentNotFound(loginName);
                }
                return Mapper.Map<IMember, MemberDisplay>(foundMember);
            }
            else
            {
                //TODO: Support this
                throw new HttpResponseException(Request.CreateValidationErrorResponse("Editing member with a non-umbraco membership provider is currently not supported"));
            }
            
        }
    }
}
