using System;

namespace Umbraco.Core.Models.Css
{
    internal enum ParseExceptionType
    {
        Warning,
        Error
    }

    [Serializable]
    internal abstract class ParseException : ApplicationException
    {
        #region Constants

        // this cannot change every char is important or Visual Studio will not list as error/warning
        // http://blogs.msdn.com/msbuild/archive/2006/11/03/msbuild-visual-studio-aware-error-messages-and-message-formats.aspx
        private const string MSBuildErrorFormat = "{0}({1},{2}): {3} {4}: {5}";

        #endregion Constants

        #region Fields

        private string file;
        private int line;
        private int column;
        private int code = 0;

        #endregion Fields

        #region Init

        public ParseException(string message, string file, int line, int column)
            : base(message)
        {
            this.file = file;
            this.line = line;
            this.column = column;
        }

        public ParseException(string message, string file, int line, int column, Exception innerException)
            : base(message, innerException)
        {
            this.file = file;
            this.line = line;
            this.column = column;
        }

        #endregion Init

        #region Properties

        public abstract ParseExceptionType Type
        {
            get;
        }

        public virtual int Code
        {
            get { return this.code; }
        }

        public string ErrorCode
        {
            get
            {
                string ext = System.IO.Path.GetExtension(file);
                if (ext == null || ext.Length < 2)
                {
                    return null;
                }

                return ext.Substring(1).ToUpperInvariant() + this.Code.ToString("####0000");
            }
        }

        public string File
        {
            get { return this.file; }
        }

        public int Line
        {
            get { return this.line; }
        }

        public int Column
        {
            get { return this.column; }
        }

        #endregion Properties

        #region Methods

        public virtual string GetCompilerMessage()
        {
            return this.GetCompilerMessage(this.Type == ParseExceptionType.Warning);
        }

        public virtual string GetCompilerMessage(bool isWarning)
        {
            // format exception as a VS2005 error/warning
            return String.Format(
                ParseException.MSBuildErrorFormat,
                this.File,
                (this.Line > 0) ? this.Line : 1,
                (this.Column > 0) ? this.Column : 1,
                isWarning ? "warning" : "error",
                this.ErrorCode,
                this.Message);
        }

        #endregion Methods
    }

    [Serializable]
    internal class ParseWarning : ParseException
    {
        #region Init

        public ParseWarning(string message, string file, int line, int column)
            : base(message, file, line, column)
        {
        }

        public ParseWarning(string message, string file, int line, int column, Exception innerException)
            : base(message, file, line, column, innerException)
        {
        }

        #endregion Init

        #region Properties

        public override ParseExceptionType Type
        {
            get { return ParseExceptionType.Warning; }
        }

        #endregion Properties
    }

    [Serializable]
    internal class ParseError : ParseException
    {
        #region Init

        public ParseError(string message, string file, int line, int column)
            : base(message, file, line, column)
        {
        }

        public ParseError(string message, string file, int line, int column, Exception innerException)
            : base(message, file, line, column, innerException)
        {
        }

        #endregion Init

        #region Properties

        public override ParseExceptionType Type
        {
            get { return ParseExceptionType.Error; }
        }

        #endregion Properties
    }

    [Serializable]
    internal class UnexpectedEndOfFile : ParseError
    {
        #region Init

        public UnexpectedEndOfFile(string message, string file, int line, int column)
            : base(message, file, line, column)
        {
        }

        public UnexpectedEndOfFile(string message, string file, int line, int column, Exception innerException)
            : base(message, file, line, column, innerException)
        {
        }

        #endregion Init
    }

    [Serializable]
    internal class FileError : ParseWarning
    {
        #region Init

        public FileError(string message, string file, int line, int column)
            : base(message, file, line, column)
        {
        }

        public FileError(string message, string file, int line, int column, Exception innerException)
            : base(message, file, line, column, innerException)
        {
        }

        #endregion Init
    }

    [Serializable]
    internal class SyntaxError : ParseError
    {
        #region Init

        public SyntaxError(string message, string file, int line, int column)
            : base(message, file, line, column)
        {
        }

        public SyntaxError(string message, string file, int line, int column, Exception innerException)
            : base(message, file, line, column, innerException)
        {
        }

        #endregion Init
    }
}