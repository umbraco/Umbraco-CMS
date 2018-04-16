using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Converts an IUserGroup instance into a dictionary of permissions by category
    /// </summary>
    internal class UserGroupDefaultPermissionsResolver
    {
        private readonly ILocalizedTextService _textService;
        private readonly ActionCollection _actions;

        public UserGroupDefaultPermissionsResolver(ILocalizedTextService textService, ActionCollection actions)
        {
            _actions = actions;
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
        }

        public IDictionary<string, IEnumerable<Permission>> Resolve(IUserGroup source)
        {
            return _actions
                .Where(x => x.CanBePermissionAssigned)
                .Select(x => GetPermission(x, source))
                .GroupBy(x => x.Category)
                .ToDictionary(x => x.Key, x => (IEnumerable<Permission>) x.ToArray());
        }

        private Permission GetPermission(IAction action, IUserGroup source)
        {
            var result = new Permission();
            var attribute = action.GetType().GetCustomAttribute<ActionMetadataAttribute>(false);
            result.Category = attribute == null
                ? _textService.Localize($"actionCategories/{Constants.Conventions.PermissionCategories.OtherCategory}")
                : _textService.Localize($"actionCategories/{attribute.Category}");
            result.Name = attribute == null || attribute.Name.IsNullOrWhiteSpace()
                ? _textService.Localize($"actions/{action.Alias}")
                : attribute.Name;
            result.Description = _textService.Localize($"actionDescriptions/{action.Alias}");
            result.Icon = action.Icon;
            result.Checked = source.Permissions != null && source.Permissions.Contains(action.Letter.ToString(CultureInfo.InvariantCulture));
            result.PermissionCode = action.Letter.ToString(CultureInfo.InvariantCulture);
            return result;
        }
    }
}
