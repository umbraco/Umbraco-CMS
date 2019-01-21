using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITemplateRepository : IReadWriteQueryRepository<int, ITemplate>
    {
        ITemplate Get(string alias);

        IEnumerable<ITemplate> GetAll(params string[] aliases);

        IEnumerable<ITemplate> GetChildren(int masterTemplateId);
        IEnumerable<ITemplate> GetChildren(string alias);

        IEnumerable<ITemplate> GetDescendants(int masterTemplateId);
        IEnumerable<ITemplate> GetDescendants(string alias);

        /// <summary>
        /// Validates a <see cref="ITemplate"/>
        /// </summary>
        /// <param name="template"><see cref="ITemplate"/> to validate</param>
        /// <returns>True if Script is valid, otherwise false</returns>
        bool ValidateTemplate(ITemplate template);

        /// <summary>
        /// Gets the content of a template as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <returns>The content of the template.</returns>
        Stream GetFileContentStream(string filepath);

        /// <summary>
        /// Sets the content of a template.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <param name="content">The content of the template.</param>
        void SetFileContent(string filepath, Stream content);

        long GetFileSize(string filepath);
    }
}
