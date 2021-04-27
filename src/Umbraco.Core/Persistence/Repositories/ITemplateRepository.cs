using System.Collections.Generic;
using System.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface ITemplateRepository : IReadWriteQueryRepository<int, ITemplate>
    {
        ITemplate Get(string alias);

        IEnumerable<ITemplate> GetAll(params string[] aliases);

        IEnumerable<ITemplate> GetChildren(int masterTemplateId);

        IEnumerable<ITemplate> GetDescendants(int masterTemplateId);

        /// <summary>
        /// Gets the content of a template as a stream.
        /// </summary>
        /// <param name="filepath">The filesystem path to the template.</param>
        /// <returns>The content of the template.</returns>
        Stream GetFileContentStream(string filepath);
    }
}
