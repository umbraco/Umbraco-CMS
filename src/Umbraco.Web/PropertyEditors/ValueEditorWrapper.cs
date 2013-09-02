using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Useful when returning a custom value editor when your property editor is attributed, it ensures the attribute
    /// values are copied across to your custom value editor.
    /// </summary>
    public class ValueEditorWrapper : ValueEditor
    {
        public ValueEditorWrapper(ValueEditor wrapped)
        {
            this.View = wrapped.View;
            this.ValueType = wrapped.ValueType;
            foreach (var v in wrapped.Validators)
            {
                Validators.Add(v);
            }
        }
    }
}