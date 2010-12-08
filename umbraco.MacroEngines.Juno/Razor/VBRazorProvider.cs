namespace umbraco.MacroEngines.Razor
{
    using System.CodeDom.Compiler;
    using System.Web.Razor;

    using Microsoft.VisualBasic;

    /// <summary>
    /// Provides a razor provider that supports the VB syntax.
    /// </summary>
    public class VBRazorProvider : IRazorProvider
    {
        #region Methods
        /// <summary>
        /// Creates a code language service.
        /// </summary>
        /// <returns>Creates a language service.</returns>
        public RazorCodeLanguage CreateLanguageService()
        {
            return new VBRazorCodeLanguage();
        }

        /// <summary>
        /// Creates a <see cref="CodeDomProvider"/>.
        /// </summary>
        /// <returns>The a code dom provider.</returns>
        public CodeDomProvider CreateCodeDomProvider()
        {
            return new VBCodeProvider();
        }
        #endregion
    }
}