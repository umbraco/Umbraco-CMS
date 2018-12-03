using System;

namespace Umbraco.ModelsBuilder.Building
{
    public class CompilerException : Exception
    {
        public CompilerException(string message)
            : base(message)
        { }

        public CompilerException(string message, string path, string sourceCode, int line)
            : base(message)
        {
            Path = path;
            SourceCode = sourceCode;
            Line = line;
        }

        public string Path { get; } = string.Empty;

        public string SourceCode { get; } = string.Empty;

        public int Line { get; } = -1;
    }
}
