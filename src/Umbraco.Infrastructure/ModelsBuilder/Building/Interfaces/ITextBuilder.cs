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
        /// <param name="availableTypes"></param>
        void Generate(StringBuilder sb, TypeModel typeModel, IEnumerable<TypeModel> availableTypes);

        /// <summary>
        ///     Outputs generated models to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="typeModels">The models to generate.</param>
        /// <param name="availableTypes"></param>
        /// <param name="addAssemblyMarker"></param>
        void Generate(StringBuilder sb, IEnumerable<TypeModel> typeModels, IEnumerable<TypeModel> availableTypes, bool addAssemblyMarker);
    }
}
