using Smidge.Nuglify;

namespace Umbraco.Web.Common.RuntimeMinification
{
    /// <summary>
    /// Custom Nuglify Js pre-process to specify custom nuglify options without changing the global defaults
    /// </summary>
    public class SmidgeNuglifyJs : NuglifyJs
    {
        public SmidgeNuglifyJs(NuglifySettings settings, ISourceMapDeclaration sourceMapDeclaration)
            : base(GetSettings(settings), sourceMapDeclaration)
        {
        }

        private static NuglifySettings GetSettings(NuglifySettings defaultSettings)
        {
            var nuglifyCodeSettings = defaultSettings.JsCodeSettings.CodeSettings.Clone();

            // Don't rename locals, this will kill a lot of angular stuff because we aren't correctly coding our
            // angular injection to handle minification correctly which requires declaring string named versions of all
            // dependencies injected (which is a pain). So we just turn this option off.
            nuglifyCodeSettings.LocalRenaming = NUglify.JavaScript.LocalRenaming.KeepAll;

            return new NuglifySettings(new NuglifyCodeSettings(nuglifyCodeSettings), defaultSettings.CssCodeSettings);
        }
    }
}
