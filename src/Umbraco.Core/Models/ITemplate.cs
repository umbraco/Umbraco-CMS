using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

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
        /// Set the mastertemplate
        /// </summary>
        /// <param name="masterTemplate"></param>
        void SetMasterTemplate(ITemplate masterTemplate);
    }
}
