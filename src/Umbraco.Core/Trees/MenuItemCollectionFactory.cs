using Umbraco.Cms.Core.Actions;

namespace Umbraco.Cms.Core.Trees;

public class MenuItemCollectionFactory : IMenuItemCollectionFactory
{
    private readonly ActionCollection _actionCollection;

    public MenuItemCollectionFactory(ActionCollection actionCollection) => _actionCollection = actionCollection;

    public MenuItemCollection Create() => new MenuItemCollection(_actionCollection);
}
