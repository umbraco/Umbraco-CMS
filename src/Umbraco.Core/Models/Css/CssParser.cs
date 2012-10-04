using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.Css
{
    internal class CssParser
    {
        #region Constants

        // this defines comments for CSS
        private static readonly ReadFilter[] ReadFilters = new ReadFilter[] { new ReadFilter("/*", "*/") };
        private readonly object SyncLock = new object();

        #endregion Constants

        #region Fields

        private readonly List<ParseException> errors = new List<ParseException>();
        private LineReader reader;
        private CssStyleSheet styleSheet;
        private string filePath;
        private string source;

        #endregion Fields

        #region Init

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filePath">path to source</param>
        public CssParser(string filePath)
            : this(filePath, null)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filePath">path to source</param>
        /// <param name="source">actual source</param>
        public CssParser(string filePath, string source)
        {
            this.filePath = filePath;
            this.source = source;
        }

        #endregion Init

        #region Properties

        public List<ParseException> Errors
        {
            get { return this.errors; }
        }

        public CssStyleSheet StyleSheet
        {
            get
            {
                if (this.styleSheet == null)
                {
                    lock (this.SyncLock)
                    {
                        // check again in case race condition
                        // so we don't parse twice
                        if (this.styleSheet == null)
                        {
                            this.styleSheet = this.ParseStyleSheet();
                        }
                    }
                }
                return this.styleSheet;
            }
        }

        private int Position
        {
            get { return this.reader.Position; }
        }

        #endregion Properties

        #region Parse Methods

        #region StyleSheet

        /// <summary>
        /// (BNF) stylesheet : [ CDO | CDC | S | statement ]*;
        /// </summary>
        /// <returns>CSS StyleSheet parse tree</returns>
        private CssStyleSheet ParseStyleSheet()
        {
            CssStyleSheet styleSheet = new CssStyleSheet();
            using (this.reader = new LineReader(this.filePath, this.source, CssParser.ReadFilters))
            {
                this.reader.NormalizeWhiteSpace = true;

#if DEBUG
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
#endif

                char ch;
                while (this.Read(out ch))
                {
                    switch (ch)
                    {
                        case '\uFEFF': // BOM (UTF byte order mark)
                        case '\t': //TAB
                        case '\n': //LF
                        case '\r': //CR
                        case ' ': //Space
                            {
                                // skip whitespace
                                continue;
                            }
                        case '<':
                            {
                                // CDO (Char Data Open?)
                                if (!this.Read(out ch) || ch != '-' ||
                                    !this.Read(out ch) || ch != '-')
                                {
                                    throw new SyntaxError("Expected \"<!--\"", this.reader.FilePath, this.reader.Line, this.reader.Column);
                                }
                                continue;
                            }
                        case '-':
                            {
                                // CDC (Char Data Close?)
                                if (!this.Read(out ch) || ch != '-' ||
                                    !this.Read(out ch) || ch != '>')
                                {
                                    throw new SyntaxError("Expected \"-->\"", this.reader.FilePath, this.reader.Line, this.reader.Column);
                                }
                                continue;
                            }
                        default:
                            {
                                try
                                {
                                    CssStatement statement = this.ParseStatement();
                                    styleSheet.Statements.Add(statement);
                                }
                                catch (ParseException ex)
                                {
                                    this.errors.Add(ex);

                                    while (this.Read(out ch) && ch != '}')
                                    {
                                        // restabilize on next statement
                                    }
                                }
                                continue;
                            }
                    }
                }

#if DEBUG
                watch.Stop();
                Console.WriteLine("CSS parse duration: {0} ms for {1} chars", watch.ElapsedMilliseconds, this.reader.Length);
#endif
            }

            this.reader = null;
            this.source = null;

            return styleSheet;
        }

        #endregion StyleSheet

        #region Statement

        /// <summary>
        /// (BNF) statement : ruleset | at-rule;
        /// </summary>
        /// <returns></returns>
        private CssStatement ParseStatement()
        {
            if (this.reader.Current == '@')
            {
                return this.ParseAtRule();
            }
            else
            {
                this.PutBack();
                return this.ParseRuleSet();
            }
        }

        #endregion Statement

        #region At-Rule

        /// <summary>
        /// (BNF) at-rule : ATKEYWORD S* any* [ block | ';' S* ];
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// NOTE: each at-rule might parse differently according to CSS3
        /// The @media block for example contains a block of statements
        /// while other at-rules with a block contain a block of declarations
        /// </remarks>
        private CssAtRule ParseAtRule()
        {
            CssAtRule atRule = new CssAtRule();
            int start = this.Position + 1;// start with first char of ident

            char ch;
            while (this.Read(out ch) && !Char.IsWhiteSpace(ch))
            {
                // continue consuming
            }

            atRule.Ident = this.Copy(start);

            while (this.Read(out ch) && Char.IsWhiteSpace(ch))
            {
                // consuming whitespace
            }

            start = this.Position;// start with current char
            do
            {
                switch (ch)
                {
                    case '{': //Block Begin
                        {
                            atRule.Value = this.Copy(start);

                            bool containsRuleSets = String.Equals(atRule.Ident, CssAtRule.MediaIdent, StringComparison.Ordinal);
                            while (true)
                            {
                                while (this.Read(out ch) && Char.IsWhiteSpace(ch))
                                {
                                    // consume whitespace
                                }

                                if (ch == '}')
                                {
                                    break;
                                }

                                try
                                {
                                    if (containsRuleSets)
                                    {
                                        // includes @media
                                        CssStatement statement = this.ParseStatement();
                                        atRule.Block.Values.Add(statement);
                                    }
                                    else
                                    {
                                        // includes @font-face, @page
                                        this.PutBack();
                                        CssDeclaration declaration = this.ParseDeclaration();
                                        atRule.Block.Values.Add(declaration);
                                    }
                                }
                                catch (ParseException ex)
                                {
                                    this.errors.Add(ex);

                                    while (this.Read(out ch) && ch != '}')
                                    {
                                        // restabilize on block end
                                    }
                                    break;
                                }
                            }
                            return atRule;
                        }
                    case ';': //At-Rule End
                        {
                            atRule.Value = this.Copy(start);
                            return atRule;
                        }
                }
            } while (this.Read(out ch));

            throw new UnexpectedEndOfFile("Unclosed At-Rule", this.reader.FilePath, this.reader.Line, this.reader.Column);
        }

        #endregion At-Rule

        #region RuleSet

        /// <summary>
        /// (BNF) ruleset : selector? '{' S* declaration? [ ';' S* declaration? ]* '}' S*;
        /// </summary>
        /// <returns></returns>
        private CssRuleSet ParseRuleSet()
        {
            char ch;
            CssRuleSet ruleSet = new CssRuleSet();

        ParseSelectors:
            while (true)
            {
                try
                {
                    CssSelector selector = this.ParseSelector();
                    if (selector == null)
                    {
                        break;
                    }
                    ruleSet.Selectors.Add(selector);
                }
                catch (ParseException ex)
                {
                    this.errors.Add(ex);

                    while (this.Read(out ch))
                    {
                        // restabalize on next rulset
                        switch (ch)
                        {
                            case ',':
                                {
                                    // continue parsing rest of Selectors
                                    goto ParseSelectors;
                                }
                            case '{':
                                {
                                    goto ParseDeclarations;
                                }
                            //case ':':// keep going
                            case ';':
                            case '}':
                                {
                                    throw new SyntaxError("Invalid selector list", this.reader.FilePath, this.reader.Line, this.reader.Column);
                                }
                        }
                    }
                }
            }

        ParseDeclarations:
            while (true)
            {
                try
                {
                    CssDeclaration declaration = this.ParseDeclaration();
                    if (declaration == null)
                    {
                        break;
                    }
                    ruleSet.Declarations.Add(declaration);
                }
                catch (ParseException ex)
                {
                    this.errors.Add(ex);

                    while (this.Read(out ch))
                    {
                        // restabalize on next declaration
                        switch (ch)
                        {
                            case '{':
                                {
                                    throw new SyntaxError("Invalid ruleset", this.reader.FilePath, this.reader.Line, this.reader.Column);
                                }
                            //case ':':// keep going
                            case ';':
                                {
                                    // continue parsing rest of delcarations
                                    goto ParseDeclarations;
                                }
                            case '}':
                                {
                                    // no more declarations
                                    return ruleSet;
                                }
                        }
                    }
                }
            }

            return ruleSet;
        }

        #endregion RuleSet

        #region Selector

        /// <summary>
        /// (BNF) selector: any+;
        /// </summary>
        /// <returns></returns>
        private CssSelector ParseSelector()
        {
            CssSelector selector = new CssSelector();
            char ch;

            while (this.Read(out ch) && (Char.IsWhiteSpace(ch) || ch == ','))
            {
                // skip whitespace, and empty selectors
            }

            // consume property name
            switch (ch)
            {
                case '{':
                    {
                        // no more declarations
                        return null;
                    }
                //case ':':// pseudoclass
                case ';':
                case '}':
                    {
                        throw new SyntaxError("Invalid chars in selector", this.reader.FilePath, this.reader.Line, this.reader.Column);
                    }
            }

            int start = this.Position;// start with current char

            while (this.Read(out ch))
            {
                // continue consuming selector
                switch (ch)
                {
                    case ',':
                    case '{':
                        {
                            selector.Value = this.Copy(start);
                            if (ch == '{')
                            {
                                this.PutBack();
                            }
                            return selector;
                        }
                    //case ':':// pseudoclass
                    case ';':
                    case '}':
                        {
                            throw new SyntaxError("Invalid selector", this.reader.FilePath, this.reader.Line, this.reader.Column);
                        }
                }
            }
            throw new UnexpectedEndOfFile("Unclosed ruleset", this.reader.FilePath, this.reader.Line, this.reader.Column);
        }

        #endregion Selector

        #region Declaration

        /// <summary>
        /// (BNF) declaration : property ':' S* value;
        /// (BNF) property    : IDENT S*;
        /// </summary>
        /// <returns></returns>
        private CssDeclaration ParseDeclaration()
        {
            CssDeclaration declaration = new CssDeclaration();
            char ch;

            while (this.Read(out ch) && (Char.IsWhiteSpace(ch) || ch == ';'))
            {
                // skip whitespace, and empty declarations
            }

            // consume property name
            switch (ch)
            {
                case '{':
                case ':':
                    //case ';':
                    {
                        throw new SyntaxError("Declaration missing property name", this.reader.FilePath, this.reader.Line, this.reader.Column);
                    }
                case '}':
                    {
                        // no more declarations
                        return null;
                    }
            }

            // read property, starting with current char
            int start = this.Position;
            while (this.Read(out ch) && !Char.IsWhiteSpace(ch) && ch != ':')
            {
                // consume property name
                switch (ch)
                {
                    case '{':
                    //case ':':
                    case ';':
                        {
                            throw new SyntaxError("Invalid CSS property name: " + this.Copy(start), this.reader.FilePath, this.reader.Line, this.reader.Column);
                        }
                    case '}':
                        {
                            this.PutBack();
                            goto case ';';
                        }
                }
            }
            declaration.Property = this.Copy(start);

            if (Char.IsWhiteSpace(ch))
            {
                while (this.Read(out ch) && (Char.IsWhiteSpace(ch)))
                {
                    // skip whitespace
                }
            }

            if (ch != ':')
            {
                // missing the property delim and value

                if (ch == ';' || ch == '}')
                {
                    // these are good chars for resyncing
                    // so put them back on the stream to
                    // not create subsequent errors
                    this.PutBack();
                }
                throw new SyntaxError("Expected <property> : <value>", this.reader.FilePath, this.reader.Line, this.reader.Column);
            }

            CssValueList value = this.ParseValue();
            declaration.Value = value;

            return declaration;
        }

        #endregion Declaration

        #region Value

        /// <summary>
        /// (BNF) value :	[ any | block | ATKEYWORD S* ]+;
        /// (BNF) any :		[ IDENT | NUMBER | PERCENTAGE | DIMENSION | STRING
        ///					| DELIM | URI | HASH | UNICODE-RANGE | INCLUDES
        ///					| FUNCTION S* any* ')' | DASHMATCH | '(' S* any* ')'
        ///					| '[' S* any* ']' ] S*;
        /// </summary>
        /// <returns></returns>
        private CssValueList ParseValue()
        {
            CssValueList value = new CssValueList();
            char ch;

            while (this.Read(out ch) && Char.IsWhiteSpace(ch))
            {
                // skip whitespace, and empty declarations
            }

            switch (ch)
            {
                case '{':
                case ':':
                case ';':
                case '}':
                    {
                        throw new SyntaxError("Invalid char in CSS property value: '" + ch + "'", this.reader.FilePath, this.reader.Line, this.reader.Column);
                    }
            }

            // read value, starting with current char
            int start = this.Position;
            while (this.Read(out ch))
            {
                // consume declaration value

                switch (ch)
                {
                    case '{':
                        //case ':':// leave in for "filter: progid:DXImageTransform.Microsoft..."
                        {
                            throw new SyntaxError("Invalid CSS property value: " + this.Copy(start), this.reader.FilePath, this.reader.Line, this.reader.Column);
                        }
                    case '}':
                    case ';':
                        {
                            //Should this parse the value further?

                            CssString any = new CssString();
                            any.Value = this.Copy(start);
                            value.Values.Add(any);
                            if (ch == '}')
                            {
                                this.PutBack();
                            }
                            return value;
                        }
                }
            }
            throw new UnexpectedEndOfFile("Unclosed declaration", this.reader.FilePath, this.reader.Line, this.reader.Column);
        }

        #endregion Value

        #endregion Parse Methods

        #region Methods

        public void Write(TextWriter writer, CssCompactor.Options options)
        {
            this.StyleSheet.Write(writer, options);
        }

        #endregion Methods

        #region Reader Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>Success</returns>
        private bool Read(out char ch)
        {
            if (this.reader.EndOfFile)
            {
                throw new UnexpectedEndOfFile("Unexpected end of file", this.reader.FilePath, this.reader.Line, this.reader.Column);
            }

            int c = this.reader.Read();
            if (c < 0)
            {
                ch = '\0';
                return false;
            }
            ch = (char)c;
            return true;
        }

        /// <summary>
        /// Copies chars from start until the position before the current position
        /// </summary>
        /// <returns></returns>
        private string Copy(int start)
        {
            // read block
            return this.reader.Copy(start, this.reader.Position - 1);
        }

        /// <summary>
        /// Put one character back
        /// </summary>
        private void PutBack()
        {
            this.reader.PutBack();
        }

        #endregion Reader Methods
    }
}