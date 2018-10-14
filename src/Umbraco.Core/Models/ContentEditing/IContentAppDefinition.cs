﻿namespace Umbraco.Core.Models.ContentEditing
{
    /// <summary>
    /// Represents a content app definition.
    /// </summary>
    public interface IContentAppDefinition
    {
        /// <summary>
        /// Gets the content app for an object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The content app for the object, or null.</returns>
        /// <remarks>
        /// <para>The definition must determine, based on <paramref name="source"/>, whether
        /// the content app should be displayed or not, and return either a <see cref="ContentApp"/>
        /// instance, or null.</para>
        /// </remarks>
        ContentApp GetContentAppFor(object source);
    }
}
