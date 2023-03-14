namespace Umbraco.Cms.Core.Models.Packaging;

public class InstallWarnings
{
    // TODO: Shouldn't we detect other conflicting entities too ?
    public IEnumerable<IMacro?>? ConflictingMacros { get; set; } = Enumerable.Empty<IMacro>();

    public IEnumerable<ITemplate>? ConflictingTemplates { get; set; } = Enumerable.Empty<ITemplate>();

    public IEnumerable<IFile?>? ConflictingStylesheets { get; set; } = Enumerable.Empty<IFile>();
}
