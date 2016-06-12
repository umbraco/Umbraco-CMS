using Umbraco.Core.Services;

namespace Umbraco.Core.Strategies
{
    public sealed class NestedContentCacheHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            DataTypeService.Saved += (sender, args) =>
            {
                foreach (var dataType in args.SavedEntities)
                {
                    ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                        string.Concat("Umbraco.PropertyEditors.NestedContent.GetPreValuesCollectionByDataTypeId_", dataType.Id));
                }
            };
        }
    }
}
