// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Trees;

namespace Umbraco.Extensions;

public static class TreeNodeExtensions
{
    internal const string LegacyJsCallbackKey = "jsClickCallback";

    /// <summary>
    ///     Sets the node style to show that it is a container type
    /// </summary>
    /// <param name="treeNode"></param>
    public static void SetContainerStyle(this TreeNode treeNode)
    {
        if (treeNode.CssClasses.Contains("is-container") == false)
        {
            treeNode.CssClasses.Add("is-container");
        }
    }

    /// <summary>
    ///     Legacy tree node's assign a JS method callback for when an item is clicked, this method facilitates that.
    /// </summary>
    /// <param name="treeNode"></param>
    /// <param name="jsCallback"></param>
    internal static void AssignLegacyJsCallback(this TreeNode treeNode, string jsCallback) =>
        treeNode.AdditionalData[LegacyJsCallbackKey] = jsCallback;

    /// <summary>
    ///     Sets the node style to show that it is currently protected publicly
    /// </summary>
    /// <param name="treeNode"></param>
    public static void SetProtectedStyle(this TreeNode treeNode)
    {
        if (treeNode.CssClasses.Contains("protected") == false)
        {
            treeNode.CssClasses.Add("protected");
        }
    }

    /// <summary>
    ///     Sets the node style to show that it is currently locked / non-deletable
    /// </summary>
    /// <param name="treeNode"></param>
    public static void SetLockedStyle(this TreeNode treeNode)
    {
        if (treeNode.CssClasses.Contains("locked") == false)
        {
            treeNode.CssClasses.Add("locked");
        }
    }

    /// <summary>
    ///     Sets the node style to show that it is has unpublished versions (but is currently published)
    /// </summary>
    /// <param name="treeNode"></param>
    public static void SetHasPendingVersionStyle(this TreeNode treeNode)
    {
        if (treeNode.CssClasses.Contains("has-unpublished-version") == false)
        {
            treeNode.CssClasses.Add("has-unpublished-version");
        }
    }

    /// <summary>
    ///     Sets the node style to show that it is not published
    /// </summary>
    /// <param name="treeNode"></param>
    public static void SetNotPublishedStyle(this TreeNode treeNode)
    {
        if (treeNode.CssClasses.Contains("not-published") == false)
        {
            treeNode.CssClasses.Add("not-published");
        }
    }
}
