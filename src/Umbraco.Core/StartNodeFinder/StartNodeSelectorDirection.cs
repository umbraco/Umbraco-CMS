namespace Umbraco.Cms.Core.StartNodeFinder;

//TODO replace with string to make it dynamic
public enum StartNodeSelectorDirection
{
    //Self,
    Parent,
    NearestAncestorOrSelf,
    FarthestAncestorOrSelf,
    NearestAncestor,
    FarthestAncestor,
    Child,
    NearestDescendants,
    FarthestDescendants,
    NearestDescendantsOrSelf,
    FarthestDescendantsOrSelf
}
