using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Converts an IUserGroup instance into a dictionary of permissions by category
    /// </summary>
    internal class UserGroupDefaultPermissionsResolver : ValueResolver<IUserGroup, IDictionary<string, IEnumerable<Permission>>>
    {
        private readonly ILocalizedTextService _textService;

        public UserGroupDefaultPermissionsResolver(ILocalizedTextService textService)
        {
            if (textService == null) throw new ArgumentNullException("textService");
            _textService = textService;
        }

        protected override IDictionary<string, IEnumerable<Permission>> ResolveCore(IUserGroup source)
        {
            return ActionsResolver.Current.Actions
                .Where(x => x.CanBePermissionAssigned)
                .Select(x => GetPermission(x, source))
                .GroupBy(x => x.Category)
                .ToDictionary(x => x.Key, x => (IEnumerable<Permission>)x.ToArray());
        }

        private Permission GetPermission(IAction action, IUserGroup source)
        {
            var result = new Permission();
            var attribute = action.GetType().GetCustomAttribute<ActionMetadataAttribute>(false);
            result.Category = attribute == null
                ? _textService.Localize(string.Format("actionCategories/{0}", Constants.Conventions.PermissionCategories.OtherCategory))
                : _textService.Localize(string.Format("actionCategories/{0}", attribute.Category));
            result.Name = attribute == null || attribute.Name.IsNullOrWhiteSpace()
                ? _textService.Localize(string.Format("actions/{0}", action.Alias))
                : attribute.Name;
            result.Description = _textService.Localize(String.Format("actionDescriptions/{0}", action.Alias));
            result.Icon = action.Icon;
            result.Checked = source.Permissions != null && source.Permissions.Contains(action.Letter.ToString(CultureInfo.InvariantCulture));
            result.PermissionCode = action.Letter.ToString(CultureInfo.InvariantCulture);
            return result;
        }
    }
}