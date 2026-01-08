namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Definition of relation directions used as a filter when requesting if a given item has relations.
/// </summary>
[Flags]
public enum RelationDirectionFilter
{
    Parent = 1,
    Child = 2,
    Any = Parent | Child
}
