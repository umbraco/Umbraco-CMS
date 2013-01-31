using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Models.Css
{
    #region Base Types

    /// <summary>
    /// CSS3 inconsistently specifies more than one grammar:
    /// http://www.w3.org/TR/css3-syntax/#style
    /// http://www.w3.org/TR/css3-syntax/#detailed-grammar
    /// </summary>
    internal abstract class CssSyntax
    {
        #region Methods

        public abstract void Write(TextWriter writer, CssCompactor.Options options);

        protected static bool IsPrettyPrint(CssCompactor.Options options)
        {
            return (options & CssCompactor.Options.PrettyPrint) > 0;
        }

        #endregion Methods

        #region Object Overrides

        public override string ToString()
        {
            var writer = new StringWriter();

            this.Write(writer, CssCompactor.Options.PrettyPrint);

            return writer.ToString();
        }

        #endregion Object Overrides
    }

    internal interface ICssValue
    {
        #region Methods

        void Write(TextWriter writer, CssCompactor.Options options);

        #endregion Methods
    }

    internal class CssString : CssSyntax
    {
        #region Fields

        private string value;

        #endregion Fields

        #region Properties

        public virtual string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            writer.Write(this.Value);
        }

        #endregion Methods
    }

    #endregion Base Types

    #region Grammar

    internal class CssStyleSheet : CssSyntax
    {
        #region Fields

        private readonly List<CssStatement> statements = new List<CssStatement>();

        #endregion Fields

        #region Properties

        public List<CssStatement> Statements
        {
            get { return this.statements; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool prettyPrint = IsPrettyPrint(options);

            foreach (CssStatement statement in this.statements)
            {
                statement.Write(writer, options);
                if (prettyPrint)
                {
                    writer.WriteLine();
                }
            }
        }

        #endregion Methods
    }

    internal abstract class CssStatement : CssSyntax, ICssValue
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// NOTE: each at-rule might parse differently according to CSS3
    /// The @media block for example contains a block of statements
    /// while other at-rules with a block contain a block of declarations
    /// </remarks>
    internal class CssAtRule : CssStatement
    {
        #region Constants

        internal const string MediaIdent = "media";

        #endregion Constants

        #region Fields

        private string ident;
        private string value;

        private CssBlock block;

        #endregion Fields

        #region Properties

        public string Ident
        {
            get { return this.ident; }
            set { this.ident = value; }
        }

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public CssBlock Block
        {
            get
            {
                if (this.block == null)
                {
                    this.block = new CssBlock();
                }
                return this.block;
            }
            set { this.block = value; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool prettyPrint = IsPrettyPrint(options);

            writer.Write('@');
            writer.Write(this.ident);

            if (!String.IsNullOrEmpty(this.value))
            {
                writer.Write(' ');
                writer.Write(this.value);
            }

            if (this.block != null)
            {
                if (prettyPrint)
                {
                    writer.WriteLine();
                }
                this.block.Write(writer, options);
            }
            else
            {
                writer.Write(';');
            }

            if (prettyPrint)
            {
                writer.WriteLine();
            }
        }

        #endregion Methods
    }

    internal class CssBlock : CssSyntax, ICssValue
    {
        #region Fields

        private readonly List<ICssValue> values = new List<ICssValue>();

        #endregion Fields

        #region Properties

        public List<ICssValue> Values
        {
            get { return this.values; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool prettyPrint = IsPrettyPrint(options);

            writer.Write('{');
            if (prettyPrint)
            {
                writer.WriteLine();
            }

            foreach (ICssValue value in this.Values)
            {
                value.Write(writer, options);
            }

            if (prettyPrint)
            {
                writer.WriteLine();
            }
            writer.Write('}');
        }

        #endregion Methods
    }

    internal class CssRuleSet : CssStatement
    {
        #region Fields

        private readonly List<CssSelector> selectors = new List<CssSelector>();
        private readonly List<CssDeclaration> declarations = new List<CssDeclaration>();

        #endregion Fields

        #region Properties

        public List<CssSelector> Selectors
        {
            get { return this.selectors; }
        }

        public List<CssDeclaration> Declarations
        {
            get { return this.declarations; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool prettyPrint = IsPrettyPrint(options);

            bool comma = false;

            foreach (CssString selector in this.Selectors)
            {
                if (comma)
                {
                    writer.Write(",");
                    if (prettyPrint)
                    {
                        writer.WriteLine();
                    }
                }
                else
                {
                    comma = true;
                }

                selector.Write(writer, options);
            }

            if (prettyPrint)
            {
                writer.WriteLine();
            }
            writer.Write("{");
            if (prettyPrint)
            {
                writer.WriteLine();
            }

            foreach (CssDeclaration dec in this.Declarations)
            {
                dec.Write(writer, options);
            }

            writer.Write("}");
            if (prettyPrint)
            {
                writer.WriteLine();
            }
        }

        #endregion Methods
    }

    internal class CssSelector : CssString
    {
    }

    internal class CssDeclaration : CssSyntax, ICssValue
    {
        #region Fields

        private string property;
        private CssValueList value;

        #endregion Fields

        #region Properties

        public string Property
        {
            get { return this.property; }
            set { this.property = value; }
        }

        public CssValueList Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool prettyPrint = IsPrettyPrint(options);
            if (prettyPrint)
            {
                writer.Write('\t');
            }
            writer.Write(this.Property);
            writer.Write(':');
            if (prettyPrint)
            {
                writer.Write(" ");
            }
            this.Value.Write(writer, options);
            writer.Write(";");
            if (prettyPrint)
            {
                writer.WriteLine();
            }
        }

        #endregion Methods
    }

    internal class CssValueList : CssSyntax
    {
        #region Fields

        private readonly List<CssString> values = new List<CssString>();

        #endregion Fields

        #region Properties

        public List<CssString> Values
        {
            get { return this.values; }
        }

        #endregion Properties

        #region Methods

        public override void Write(TextWriter writer, CssCompactor.Options options)
        {
            bool space = false;

            foreach (CssString value in this.Values)
            {
                if (space)
                {
                    writer.Write(" ");
                }
                else
                {
                    space = true;
                }

                value.Write(writer, options);
            }
        }

        #endregion Methods
    }

    #endregion Grammar
}