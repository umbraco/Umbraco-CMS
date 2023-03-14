namespace Umbraco.Cms.Core.Collections;

public enum ListCloneBehavior
{
    /// <summary>
    ///     When set, DeepClone will clone the items one time and the result list behavior will be None
    /// </summary>
    CloneOnce,

    /// <summary>
    ///     When set, DeepClone will not clone any items
    /// </summary>
    None,

    /// <summary>
    ///     When set, DeepClone will always clone all items
    /// </summary>
    Always,
}
