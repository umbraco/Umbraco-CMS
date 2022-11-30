using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Sections;

namespace Umbraco.Cms.Core.Services;

public class SectionService : ISectionService
{
    private readonly SectionCollection _sectionCollection;
    private readonly IUserService _userService;

    public SectionService(
        IUserService userService,
        SectionCollection sectionCollection)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _sectionCollection = sectionCollection ?? throw new ArgumentNullException(nameof(sectionCollection));
    }

    /// <summary>
    ///     The cache storage for all applications
    /// </summary>
    public IEnumerable<ISection> GetSections()
        => _sectionCollection;

    /// <inheritdoc />
    public IEnumerable<ISection> GetAllowedSections(int userId)
    {
        IUser? user = _userService.GetUserById(userId);
        if (user == null)
        {
            throw new InvalidOperationException("No user found with id " + userId);
        }

        return GetSections().Where(x => user.AllowedSections.Contains(x.Alias));
    }

    /// <inheritdoc />
    public ISection? GetByAlias(string appAlias)
        => GetSections().FirstOrDefault(t => t.Alias.Equals(appAlias, StringComparison.OrdinalIgnoreCase));
}
