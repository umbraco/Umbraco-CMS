using System.Collections.Generic;

namespace Umbraco.Core.Security
{
    public class FileStreamSecurityValidatorFactory
    {
        private static List<IFileStreamSecurityAnalyzer> _fileAnalyzers = new List<IFileStreamSecurityAnalyzer>();

        public IFileStreamSecurityValidator CreateValidator()
        {
            return new FileStreamSecurityValidator(_fileAnalyzers);
        }

        public void AddAnalyzer(IFileStreamSecurityAnalyzer analyzer)
        {
            // We don't want duplicates
            if (_fileAnalyzers.Contains(analyzer) == false)
            {
                _fileAnalyzers.Add(analyzer);
            }
        }

        public void RemoveAnalyzer(IFileStreamSecurityAnalyzer analyzer)
        {
            _fileAnalyzers.Remove(analyzer);
        }
    }
}
