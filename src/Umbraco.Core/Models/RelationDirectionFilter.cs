namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Definition of relation directions used as a filter when requesting if a given item has relations.
/// </summary>
[Flags]
public enum RelationDirectionFilter
{
    /// <summary>
    /// Filter to include relations where the item is the parent.
    /// </summary>
    Parent = 1,

    /// <summary>
    /// Filter to include relations where the item is the child.
    /// </summary>
    Child = 2,

    /// <summary>
    /// Filter to include relations where the item is either the parent or the child.
    /// </summary>
    Any = Parent | Child
}
