using System.IO;

namespace Umbraco.Core.Security
{
    public interface IFileStreamSecurityValidator
    {
        /// <summary>
        /// Analyzes whether the file content is considered safe with registered IFileStreamSecurityAnalyzers
        /// </summary>
        /// <param name="fileStream">Needs to be a Read seekable stream</param>
        /// <returns>Whether the file is considered safe after running the necessary analyzers</returns>
        bool IsConsideredSafe(Stream fileStream);
    }
}
