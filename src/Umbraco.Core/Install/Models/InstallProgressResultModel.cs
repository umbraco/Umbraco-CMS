using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models;

/// <summary>
///     Returned to the UI for each installation step that is completed
/// </summary>
[Obsolete("Will no longer be required with the new backoffice API")]
[DataContract(Name = "result", Namespace = "")]
public class InstallProgressResultModel
{
    public InstallProgressResultModel(bool processComplete, string stepCompleted, string nextStep, string? view = null, object? viewModel = null)
    {
        ProcessComplete = processComplete;
        StepCompleted = stepCompleted;
        NextStep = nextStep;
        ViewModel = viewModel;
        View = view;
    }

    /// <summary>
    ///     The UI view to show when this step executes, by default no views are shown for the completion of a step unless
    ///     explicitly specified.
    /// </summary>
    [DataMember(Name = "view")]
    public string? View { get; private set; }

    [DataMember(Name = "complete")]
    public bool ProcessComplete { get; set; }

    [DataMember(Name = "stepCompleted")]
    public string StepCompleted { get; set; }

    [DataMember(Name = "nextStep")]
    public string NextStep { get; set; }

    /// <summary>
    ///     The view model to return to the UI if this step is returning a view (optional)
    /// </summary>
    [DataMember(Name = "model")]
    public object? ViewModel { get; private set; }
}
