namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Abstract class that all Property editors should inherit from
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    /// <typeparam name="TPreValueModel"></typeparam>
    /// <typeparam name="TValueModel"> </typeparam>
    internal abstract class PropertyEditor<TValueModel, TEditorModel, TPreValueModel> : PropertyEditor
        where TValueModel : IValueModel, new() 
        where TEditorModel : EditorModel<TValueModel>
        where TPreValueModel : PreValueModel
    {

        /// <summary>
        /// Returns the editor model to be used for the property editor
        /// </summary>
        /// <returns></returns>
        public abstract TEditorModel CreateEditorModel(TPreValueModel preValues);

        /// <summary>
        /// Returns the editor model to be used for the prevalue editor
        /// </summary>
        /// <returns></returns>
        public abstract TPreValueModel CreatePreValueEditorModel();
    }

    /// <summary>
    /// Abstract class that Property editors should inherit from that don't require a pre-value editor
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    /// <typeparam name="TValueModel"> </typeparam>
    internal abstract class PropertyEditor<TValueModel, TEditorModel> : PropertyEditor<TValueModel, TEditorModel, BlankPreValueModel>
        where TValueModel : IValueModel, new() 
        where TEditorModel : EditorModel<TValueModel>
    {
        public override BlankPreValueModel CreatePreValueEditorModel()
        {
            return new BlankPreValueModel();
        }

        public sealed override TEditorModel CreateEditorModel(BlankPreValueModel preValues)
        {
            return CreateEditorModel();
        }

        public abstract TEditorModel CreateEditorModel();
    }
}