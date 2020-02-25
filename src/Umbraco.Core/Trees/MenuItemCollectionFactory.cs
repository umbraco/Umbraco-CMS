using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public class MenuItemCollectionFactory: IMenuItemCollectionFactory
    {
        private readonly ActionCollection _actionCollection;

        public MenuItemCollectionFactory(ActionCollection actionCollection)
        {
            _actionCollection = actionCollection;
        }

        public MenuItemCollection Create()
        {
            return new MenuItemCollection(_actionCollection);
        }

    }
}
