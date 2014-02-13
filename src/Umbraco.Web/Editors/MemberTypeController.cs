using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    public class MemberTypeController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MemberTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MemberTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns all member types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAllTypes()
        {
            return Services.MemberTypeService.GetAll()
                           .Select(Mapper.Map<IMemberType, ContentTypeBasic>);
        }
    }
}