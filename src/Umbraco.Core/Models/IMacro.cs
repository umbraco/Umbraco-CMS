using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Macro
    /// </summary>
    public interface IMacro : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
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
        /// Gets or sets the path to user control or the Control Type to render
        /// </summary>
        [DataMember]
        string ControlType { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly, which should be used by the Macro
        /// </summary>
        /// <remarks>Will usually only be filled if the ScriptFile is a Usercontrol</remarks>
        [DataMember]
        string ControlAssembly { get; set; }

        /// <summary>
        /// Gets or set the path to the Python file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        string ScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the Xslt file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        string XsltPath { get; set; }

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