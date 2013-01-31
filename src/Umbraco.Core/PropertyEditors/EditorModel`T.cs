namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An abstract class representing the model to render a Property Editor's content editor with PreValues
    /// </summary>
    /// <typeparam name="TPreValueModel">The type of the PreValue model.</typeparam>
    /// <typeparam name="TValueModel"> </typeparam>
    internal abstract class EditorModel<TValueModel, TPreValueModel> : EditorModel<TValueModel>
        where TValueModel : IValueModel, new()
        where TPreValueModel : PreValueModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="preValues">The pre value options used to construct the editor</param>
        protected EditorModel(TPreValueModel preValues)
        {
            PreValueModel = preValues;
        }

        /// <summary>
        /// The pre value options used to configure the editor
        /// </summary>
        /// <value>The pre value model.</value>
        public TPreValueModel PreValueModel { get; protected internal set; }
    }
}