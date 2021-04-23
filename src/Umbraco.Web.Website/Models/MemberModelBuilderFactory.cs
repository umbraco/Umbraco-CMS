using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.Website.Models
{
    /// <summary>
    /// Service to create model builder instances for working with Members on the front-end
    /// </summary>
    public class MemberModelBuilderFactory
    {
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMemberService _memberService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberModelBuilderFactory(IMemberTypeService memberTypeService, IMemberService memberService, IShortStringHelper shortStringHelper, IHttpContextAccessor httpContextAccessor)
        {
            _memberTypeService = memberTypeService;
            _memberService = memberService;
            _shortStringHelper = shortStringHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Create a <see cref="RegisterModelBuilder"/>
        /// </summary>
        /// <returns></returns>
        public RegisterModelBuilder CreateRegisterModel() => new RegisterModelBuilder(_memberTypeService, _shortStringHelper);

        /// <summary>
        /// Create a <see cref="RegisterModelBuilder"/>
        /// </summary>
        /// <returns></returns>
        public ProfileModelBuilder CreateProfileModel() => new ProfileModelBuilder(_memberTypeService, _memberService, _shortStringHelper, _httpContextAccessor);
    }
}
