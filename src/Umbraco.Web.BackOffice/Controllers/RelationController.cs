using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    public class RelationController : UmbracoAuthorizedJsonController
    {
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IRelationService _relationService;

        public RelationController(IUmbracoMapper umbracoMapper,
            IRelationService relationService)
        {
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        }

        public RelationDisplay? GetById(int id)
        {
            return _umbracoMapper.Map<IRelation, RelationDisplay>(_relationService.GetById(id));
        }

        //[EnsureUserPermissionForContent("childId")]
        public IEnumerable<RelationDisplay> GetByChildId(int childId, string relationTypeAlias = "")
        {
            var relations = _relationService.GetByChildId(childId).ToArray();

            if (relations.Any() == false)
            {
                return Enumerable.Empty<RelationDisplay>();
            }

            if (string.IsNullOrWhiteSpace(relationTypeAlias) == false)
            {
                return
                    _umbracoMapper.MapEnumerable<IRelation, RelationDisplay>(
                        relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias))).WhereNotNull();
            }

            return _umbracoMapper.MapEnumerable<IRelation, RelationDisplay>(relations).WhereNotNull();
        }

    }
}
