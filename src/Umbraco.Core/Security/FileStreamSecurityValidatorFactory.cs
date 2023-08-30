using System.Collections.Generic;
using System.Linq;

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
            if (_fileAnalyzers.Any(fileAnalyzer => fileAnalyzer.GetType() == analyzer.GetType()))
            {
                return;
            }

            _fileAnalyzers.Add(analyzer);
        }

        public void RemoveAnalyzer(IFileStreamSecurityAnalyzer analyzer)
        {
            _fileAnalyzers.Remove(analyzer);
        }
    }
}
