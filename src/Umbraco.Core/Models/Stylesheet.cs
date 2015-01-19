using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Css;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Stylesheet file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Stylesheet : File
    {
        public Stylesheet(string path) : base(path)
        {
            base.Path = path;
        }

        /// <summary>
        /// Returns a flat list of <see cref="StylesheetProperty"/> objects
        /// </summary>
        /// <remarks>
        /// Please note that the list is flattend by formatting single css selectors with
        /// its value(s). Blocks in css @ rules are also flatten, but noted as part of an @ rule
        /// by setting the <see cref="StylesheetProperty"/> property IsPartOfAtRule=true.
        /// This is done to make the stylesheet usable in the backoffice.
        /// </remarks>
        [IgnoreDataMember]
        public IEnumerable<StylesheetProperty> Properties
        {
            get
            {
                var properties = new List<StylesheetProperty>();
                var parser = new CssParser(Content);

                foreach (var statement in parser.StyleSheet.Statements.OfType<CssAtRule>())
                {
                    var cssBlock = statement.Block;
                    if(cssBlock == null) continue;

                    var cssValues = cssBlock.Values;
                    if(cssValues == null) continue;

                    properties.AddRange(FormatCss(cssBlock.Values, true));
                }

                var statements = parser.StyleSheet.Statements.Where(s => s is CssRuleSet);
                properties.AddRange(FormatCss(statements, false));

                return properties;
            }
        }

        /// <summary>
        /// Formats a list of statements to a simple <see cref="StylesheetProperty"/> object
        /// </summary>
        /// <param name="statements">Enumerable list of <see cref="ICssValue"/> statements</param>
        /// <param name="isPartOfAtRule">Boolean indicating whether the current list of statements is part of an @ rule</param>
        /// <returns>An Enumerable list of <see cref="StylesheetProperty"/> objects</returns>
        private IEnumerable<StylesheetProperty> FormatCss(IEnumerable<ICssValue> statements, bool isPartOfAtRule)
        {
            var properties = new List<StylesheetProperty>();

            foreach (var statement in statements.OfType<CssRuleSet>())
            {
                foreach (var selector in statement.Selectors)
                {
                    var declarations = new StringBuilder();
                    foreach (var declaration in statement.Declarations)
                    {
                        declarations.AppendFormat("{0}:{1};", declaration.Property, FormatCss(declaration.Value));
                        declarations.AppendLine("");
                    }
                    properties.Add(new StylesheetProperty(selector.Value.TrimStart('.', '#'), declarations.ToString()) { IsPartOfAtRule = isPartOfAtRule });
                }
            }

            return properties;
        }

        /// <summary>
        /// Formats a <see cref="CssValueList"/> to a single string
        /// </summary>
        /// <param name="valueList"><see cref="CssValueList"/> to format</param>
        /// <returns>Value list formatted as a string</returns>
        private string FormatCss(CssValueList valueList)
        {
            bool space = false;
            var values = new StringBuilder();

            foreach (CssString value in valueList.Values)
            {
                if (space)
                {
                    values.Append(" ");
                }
                else
                {
                    space = true;
                }

                values.Append(value);
            }

            return values.ToString();
        }
        
        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        //TODO: This makes no sense to be here, any validation methods should be at the service level,
        // when we move Scripts to truly use IFileSystem, then this validation logic doesn't work anymore
        public override bool IsValid()
        {
            var dirs = SystemDirectories.Css;

            //Validate file
            var validFile = IOHelper.VerifyEditPath(Path, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.VerifyFileExtension(Path, new List<string> {"css"});

            return validFile && validExtension;
        }

        /// <summary>
        /// Boolean indicating whether the file is valid css using a css parser
        /// </summary>
        /// <returns>True if css is valid, otherwise false</returns>
        public bool IsFileValidCss()
        {
            var parser = new CssParser(Content);

            try
            {
                var styleSheet = parser.StyleSheet;//Get stylesheet to invoke parsing
            }
            catch (Exception ex)
            {
                //Log exception?
                return false;
            }
            
            return !parser.Errors.Any();
        }

        /// <summary>
        /// Indicates whether the current entity has an identity, which in this case is a path/name.
        /// </summary>
        /// <remarks>
        /// Overrides the default Entity identity check.
        /// </remarks>
        public override bool HasIdentity
        {
            get { return string.IsNullOrEmpty(Path) == false; }
        }
    }
}