namespace Umbraco.Cms.Core.PropertyEditors
{

    public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("minNumber", "Minimum number of items", "number")]
        public int MinNumber { get; set; }

        [ConfigurationField("maxNumber", "Maximum number of items", "number")]
        public int MaxNumber { get; set; }

        [ConfigurationField("overlayWidthSize", "Overlay width size", "views/propertyeditors/multiurlpicker/multiurlpicker.prevalues.html")]
        public string OverlayWidthSize { get; set; }

        [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore user start nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }

        [ConfigurationField("hideAnchor",
            "Hide anchor/query string input", "boolean",
            Description = "Selecting this hides the anchor/query string input field in the linkpicker overlay.")]
        public bool HideAnchor { get; set; }
    }
}
