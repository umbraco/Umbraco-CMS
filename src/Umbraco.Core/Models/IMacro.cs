using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Macro
    /// </summary>
    public interface IMacro : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the alias of the Macro
        /// </summary>
        [DataMember]
        string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the Macro
        /// </summary>
        [DataMember]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro can be used in an Editor
        /// </summary>
        [DataMember]
        bool UseInEditor { get; set; }

        /// <summary>
        /// Gets or sets the Cache Duration for the Macro
        /// </summary>
        [DataMember]
        int CacheDuration { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached by Page
        /// </summary>
        [DataMember]
        bool CacheByPage { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached Personally
        /// </summary>
        [DataMember]
        bool CacheByMember { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be rendered in an Editor
        /// </summary>
        [DataMember]
        bool DontRender { get; set; }
                
        /// <summary>
        /// Gets or set the path to the macro source to render
        /// </summary>
        [DataMember]
        string MacroSource { get; set; }

        /// <summary>
        /// Gets or set the macro type
        /// </summary>
        [DataMember]
        MacroTypes MacroType { get; set; }

        /// <summary>
        /// Gets or sets a list of Macro Properties
        /// </summary>
        [DataMember]
        MacroPropertyCollection Properties { get; }

        ///// <summary>
        ///// Returns an enum <see cref="MacroTypes"/> based on the properties on the Macro
        ///// </summary>
        ///// <returns><see cref="MacroTypes"/></returns>
        //MacroTypes MacroType();
    }
}
