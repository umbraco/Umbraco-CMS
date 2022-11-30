using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApps;

public class ContentAppFactoryCollection : BuilderCollectionBase<IContentAppFactory>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ILogger<ContentAppFactoryCollection> _logger;

    public ContentAppFactoryCollection(
        Func<IEnumerable<IContentAppFactory>> items,
        ILogger<ContentAppFactoryCollection> logger,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(items)
    {
        _logger = logger;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public IEnumerable<ContentApp> GetContentAppsFor(object o, IEnumerable<IReadOnlyUserGroup>? userGroups = null)
    {
        IEnumerable<IReadOnlyUserGroup> roles = GetCurrentUserGroups();

        var apps = this.Select(x => x.GetContentAppFor(o, roles)).WhereNotNull().OrderBy(x => x.Weight).ToList();

        var aliases = new HashSet<string>();
        List<string>? dups = null;

        foreach (ContentApp app in apps)
        {
            if (app.Alias is not null)
            {
                if (aliases.Contains(app.Alias))
                {
                    (dups ??= new List<string>()).Add(app.Alias);
                }
                else
                {
                    aliases.Add(app.Alias);
                }
            }
        }

        if (dups != null)
        {
            // dying is not user-friendly, so let's write to log instead, and wish people read logs...

            // throw new InvalidOperationException($"Duplicate content app aliases found: {string.Join(",", dups)}");
            _logger.LogWarning("Duplicate content app aliases found: {DuplicateAliases}", string.Join(",", dups));
        }

        return apps;
    }

    private IEnumerable<IReadOnlyUserGroup> GetCurrentUserGroups()
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        return currentUser == null
            ? Enumerable.Empty<IReadOnlyUserGroup>()
            : currentUser.Groups;
    }
}
