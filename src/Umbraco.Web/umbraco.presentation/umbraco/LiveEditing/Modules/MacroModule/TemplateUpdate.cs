using System;
using umbraco.presentation.LiveEditing.Updates;

namespace umbraco.presentation.LiveEditing.Modules.MacroEditing
{
    /// <summary>
    /// Class that holds information about an update to a certain template.
    /// </summary>
    /// <remarks>Not implemented yet.</remarks>
    [Serializable]
    public class TemplateUpdate : IUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateUpdate"/> class.
        /// </summary>
        public TemplateUpdate()
        {
            
        }

        #region IUpdate Members

        /// <summary>
        /// Saves the update.
        /// </summary>
        public void Save()
        {
            
        }

        /// <summary>
        /// Publishes the update.
        /// </summary>
        public void Publish()
        {

        }

        #endregion
    }
}
