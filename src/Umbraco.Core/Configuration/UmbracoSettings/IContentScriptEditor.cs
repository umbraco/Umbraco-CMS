using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentScriptEditor
    {
        string ScriptFolderPath { get; }

        IEnumerable<string> ScriptFileTypes { get; }

        bool DisableScriptEditor { get; }
    }
}