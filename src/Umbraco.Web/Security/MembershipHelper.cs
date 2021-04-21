using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Security
{
    // MIGRATED TO NETCORE
    public class MembershipHelper
    {
        private readonly MembersMembershipProvider _membershipProvider;
        private readonly RoleProvider _roleProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly AppCaches _appCaches;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IEntityService _entityService;

        #region Constructors

        public MembershipHelper
        (
            IHttpContextAccessor httpContextAccessor,
            IPublishedMemberCache memberCache,
            MembersMembershipProvider membershipProvider,
            RoleProvider roleProvider,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IPublicAccessService publicAccessService,
            AppCaches appCaches,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEntityService entityService
        )
        {
            MemberCache = memberCache;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _publicAccessService = publicAccessService;
            _appCaches = appCaches;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<MembershipHelper>();
            _shortStringHelper = shortStringHelper;

            _membershipProvider = membershipProvider ?? throw new ArgumentNullException(nameof(membershipProvider));
            _roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        #endregion

        protected IPublishedMemberCache MemberCache { get; }

        #region Querying for front-end

        public virtual IPublishedContent GetByProviderKey(object key)
        {
            return MemberCache.GetByProviderKey(key);
        }

        public virtual IEnumerable<IPublishedContent> GetByProviderKeys(IEnumerable<object> keys)
        {
            return keys?.Select(GetByProviderKey).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();
        }

        public virtual IPublishedContent GetById(int memberId)
        {
            return MemberCache.GetById(memberId);
        }

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<int> memberIds)
        {
            return memberIds?.Select(GetById).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();
        }

        public virtual IPublishedContent GetById(Guid memberId)
        {
            return GetByProviderKey(memberId);
        }

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> memberIds)
        {
            return GetByProviderKeys(memberIds.OfType<object>());
        }

        public virtual IPublishedContent GetByUsername(string username)
        {
            return MemberCache.GetByUsername(username);
        }

        public virtual IPublishedContent GetByEmail(string email)
        {
            return MemberCache.GetByEmail(email);
        }

        public virtual IPublishedContent Get(Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null)
                return null;

            var umbracoType = UdiEntityTypeHelper.ToUmbracoObjectType(udi.EntityType);

            switch (umbracoType)
            {
                case UmbracoObjectTypes.Member:
                    // TODO: need to implement Get(guid)!
                    var memberAttempt = _entityService.GetId(guidUdi.Guid, umbracoType);
                    if (memberAttempt.Success)
                        return GetById(memberAttempt.Result);
                    break;
            }

            return null;
        }

        #endregion

    }
}
