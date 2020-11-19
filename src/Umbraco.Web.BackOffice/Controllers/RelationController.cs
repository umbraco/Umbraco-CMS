using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionsResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    public class RelationController : UmbracoAuthorizedJsonController
    {
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IRelationService _relationService;

        public RelationController(UmbracoMapper umbracoMapper,
            IRelationService relationService)
        {
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        }

        public RelationDisplay GetById(int id)
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
                        relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)));
            }

            return _umbracoMapper.MapEnumerable<IRelation, RelationDisplay>(relations);
        }

    }
}
