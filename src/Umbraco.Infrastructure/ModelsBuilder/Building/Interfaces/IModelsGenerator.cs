namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface IModelsGenerator
    {
        /// <summary>
        ///    Generates the models and writes them to disk.
        /// </summary>
        /// <param name="outputFileExtension"></param>
        void GenerateModels(string outputFileExtension);
    }
}
