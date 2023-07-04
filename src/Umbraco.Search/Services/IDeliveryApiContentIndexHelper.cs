using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.Services;

internal interface IDeliveryApiContentIndexHelper
{
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
