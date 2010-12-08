using System.Dynamic;
namespace umbraco.MacroEngines.Razor
{
    /// <summary>
    /// A razor template.
    /// </summary>
    public interface ITemplate
    {
        #region Properties
        /// <summary>
        /// Gets the parsed result of the template.
        /// </summary>
        string Result { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the template.
        /// </summary>
        void Clear();

        /// <summary>
        /// Executes the template.
        /// </summary>
        void Execute();

        /// <summary>
        /// Writes the specified object to the template.
        /// </summary>
        /// <param name="object"></param>
        void Write(object @object);

        /// <summary>
        /// Writes a literal to the template.
        /// </summary>
        /// <param name="literal"></param>
        void WriteLiteral(string literal);
        #endregion
    }

    /// <summary>
    /// A razor template with a model.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public interface ITemplate<TModel> : ITemplate
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        TModel Model { get; set; }
        #endregion
    }

    public interface ITemplateDynamic : ITemplate {
        dynamic Model { get; set; }
    }
}