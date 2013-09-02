using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Custom value editor to mark it as readonly
    /// </summary>
    internal class LabelValueEditor : ValueEditorWrapper
    {
        public LabelValueEditor(ValueEditor wrapped) : base(wrapped)
        {
        }

        /// <summary>
        /// This editor is for display purposes only, any values bound to it will not be saved back to the database
        /// </summary>
        public override bool IsReadOnly
        {
            get { return true; }
        }
    }
}