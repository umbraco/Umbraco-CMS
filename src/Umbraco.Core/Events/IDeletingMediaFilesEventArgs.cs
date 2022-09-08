namespace Umbraco.Cms.Core.Events;

public interface IDeletingMediaFilesEventArgs
{
    List<string> MediaFilesToDelete { get; }
}
