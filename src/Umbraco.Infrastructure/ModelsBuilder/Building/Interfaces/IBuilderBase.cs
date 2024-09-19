namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface IBuilderBase
    {
        /// <summary>
        ///     Gets the list of models to generate.
        /// </summary>
        /// <returns>The models to generate</returns>
        IEnumerable<TypeModel> GetModelsToGenerate();
        string GetModelsNamespace();

        /// <summary>
        ///     Returns PublishedElementModel or PublishedContentModel dependant on whether given type is an element.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetModelsBaseClassName(TypeModel type);

        /// <summary>
        ///    Gets or sets and sets the list of using directives added to the generated model
        /// </summary>
        IList<string> Using { get; set; }

        /// <summary>
        ///     Prepares generation by processing the result of code parsing.
        /// </summary>
        void Prepare(IEnumerable<TypeModel> types);
    }
}
