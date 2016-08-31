namespace Umbraco.Core.Components
{
    // the UmbracoCoreComponent requires all IUmbracoCoreComponent
    // all user-level components should require the UmbracoCoreComponent

    [RequireComponent(typeof(IUmbracoCoreComponent))]
    public class UmbracoCoreComponent : UmbracoComponentBase
    { }
}
