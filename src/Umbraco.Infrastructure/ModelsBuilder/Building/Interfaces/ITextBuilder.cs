using System.Text;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilder
    {
        /// <summary>
        ///     Outputs a generated model to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="typeModel">The model to generate.</param>
        /// <param name="availableTypes">All types available to the modelsbuilder (ie. things a model can be composed of)</param>
        void Generate(StringBuilder sb, TypeModel typeModel, IEnumerable<TypeModel> availableTypes);

        /// <summary>
        ///     Outputs generated models to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="typeModels">The models to generate.</param>
        /// <param name="availableTypes">All types available to the modelsbuilder (ie. things a model can be composed of)</param>
        /// <param name="addAssemblyMarker">Whether the modelsbuilder should add an assembly marker. Used by in-memory modelsbuilder</param>
        void Generate(StringBuilder sb, IEnumerable<TypeModel> typeModels, IEnumerable<TypeModel> availableTypes, bool addAssemblyMarker);
    }
}
