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
        RenderingEngine GetTypeOfRenderingEngine();

        /// <summary>
        /// Set the mastertemplate
        /// </summary>
        /// <param name="masterTemplate"></param>
        void SetMasterTemplate(ITemplate masterTemplate);
    }
}