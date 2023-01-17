namespace Umbraco.Cms.Api.Management.ViewModels.UserGroups;

public class UserGroupViewModel
{
    public required Guid Key { get; init; }

    public string? Name { get; init; }

    public string? Icon { get; init; }

    public string Type => "user-group";

    public required IEnumerable<string> Sections { get; init; }

    public required IEnumerable<int> Languages { get; init; }

    public required bool HasAccessToAllLanguages { get; init; }

    public Guid? ContentStartNodeKey { get; init; }

    public Guid? MediaStartNodeKey { get; init; }

    public required IEnumerable<string> Permissions { get; init; }
}
