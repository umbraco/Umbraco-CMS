namespace Umbraco.Cms.Core.Services;

public interface IDictionaryService
{
    string CalculatePath(Guid? parentId, int sourceId);
}
