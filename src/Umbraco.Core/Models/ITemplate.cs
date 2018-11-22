using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Template File (Masterpage or Mvc View)
    /// </summary>
    public interface ITemplate : IFile, IRememberBeingDirty, ICanBeDirty
    {
        /// <summary>
        /// Gets the Name of the File including extension
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets the Alias of the File, which is the name without the extension
        /// </summary>
        new string Alias { get; set; }

        /// <summary>
        /// Returns true if the template is used as a layout for other templates (i.e. it has 'children')
        /// </summary>
        bool IsMasterTemplate { get; }

        /// <summary>
        /// returns the master template alias
        /// </summary>
        string MasterTemplateAlias { get; }

        /// <summary>
        /// Returns the <see cref="RenderingEngine"/> that corresponds to the template file
        /// </summary>
        /// <returns><see cref="RenderingEngine"/></returns>
        [Obsolete("This is no longer used and will be removed from the codebase in future versions, use the IFileSystem DetermineRenderingEngine method instead")]
        RenderingEngine GetTypeOfRenderingEngine();

        /// <summary>
        /// Set the mastertemplate
        /// </summary>
        /// <param name="masterTemplate"></param>
        void SetMasterTemplate(ITemplate masterTemplate);
    }
}