using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Macro : Entity, IMacro
    {
        /// <summary>
        /// Gets or sets the alias of the Macro
        /// </summary>
        [DataMember]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the Macro
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro can be used in an Editor
        /// </summary>
        [DataMember]
        public bool UseInEditor { get; set; }

        /// <summary>
        /// Gets or sets the Cache Duration for the Macro
        /// </summary>
        [DataMember]
        public int CacheDuration { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached by Page
        /// </summary>
        [DataMember]
        public bool CacheByPage { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached Personally
        /// </summary>
        [DataMember]
        public bool CacheByMember { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be rendered in an Editor
        /// </summary>
        [DataMember]
        public bool DontRender { get; set; }

        /// <summary>
        /// Gets or sets the path to the script file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string ScriptFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly, which should be used by the Macro
        /// </summary>
        /// <remarks>Will usually only be filled if the ScriptFile is a Usercontrol</remarks>
        [DataMember]
        public string ScriptAssembly { get; set; }

        /// <summary>
        /// Gets or set the path to the Python file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string Python { get; set; }

        /// <summary>
        /// Gets or sets the path to the Xslt file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string Xslt { get; set; }

        /// <summary>
        /// Gets or sets a list of Macro Properties
        /// </summary>
        [DataMember]
        public List<IMacroProperty> Properties { get; set; }

        /// <summary>
        /// Overridden this method in order to set a random Id
        /// </summary>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            var random = new Random();
            Id = random.Next(10000, int.MaxValue);

            Key = Alias.EncodeAsGuid();
        }

        /// <summary>
        /// Returns an enum <see cref="MacroTypes"/> based on the properties on the Macro
        /// </summary>
        /// <returns><see cref="MacroTypes"/></returns>
        public MacroTypes MacroType()
        {
            if (!string.IsNullOrEmpty(Xslt))
                return MacroTypes.Xslt;
            
            if (!string.IsNullOrEmpty(Python))
                return MacroTypes.Python;
            
            if (!string.IsNullOrEmpty(ScriptFile))
                return MacroTypes.Script;
            
            if (!string.IsNullOrEmpty(ScriptFile) && ScriptFile.ToLower().IndexOf(".ascx", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return MacroTypes.UserControl;
            }
            
            if (!string.IsNullOrEmpty(ScriptFile) && !string.IsNullOrEmpty(ScriptAssembly))
                return MacroTypes.CustomControl;

            return MacroTypes.Unknown;
        }
    }
}