namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfiguration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("startNode", "Node type", "treesource")]
    public MultiNodePickerConfigurationTreeSource? TreeSource { get; set; }

    [ConfigurationField("filter", "Allow items of type", "treesourcetypepicker", Description = "Select the applicable types")]
    public string? Filter { get; set; }

    [ConfigurationField("minNumber", "Minimum number of items", "number")]
    public int MinNumber { get; set; }

    [ConfigurationField("maxNumber", "Maximum number of items", "number")]
    public int MaxNumber { get; set; }

    [ConfigurationField("showOpenButton", "Show open button", "boolean", Description = "Opens the node in a dialog")]
    public bool ShowOpen { get; set; }

    [ConfigurationField(
        Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
        "Ignore User Start Nodes",
        "boolean",
        Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
    public bool IgnoreUserStartNodes { get; set; }
}
