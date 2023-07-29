using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.Services;

public interface IDeliveryApiContentIndexHelper
{
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
