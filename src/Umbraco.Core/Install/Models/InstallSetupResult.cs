namespace Umbraco.Cms.Core.Install.Models;

/// <summary>
///     The object returned from each installation step
/// </summary>
[Obsolete("Will no longer be required with the new backoffice API")]
public class InstallSetupResult
{
    public InstallSetupResult()
    {
    }

    public InstallSetupResult(IDictionary<string, object> savedStepData, string view, object? viewModel = null)
    {
        ViewModel = viewModel;
        SavedStepData = savedStepData;
        View = view;
    }

    public InstallSetupResult(IDictionary<string, object> savedStepData) => SavedStepData = savedStepData;

    public InstallSetupResult(string view, object? viewModel = null)
    {
        ViewModel = viewModel;
        View = view;
    }

    /// <summary>
    ///     Data that is persisted to the installation file which can be used from other installation steps
    /// </summary>
    public IDictionary<string, object>? SavedStepData { get; }

    /// <summary>
    ///     The UI view to show when this step executes, by default no views are shown for the completion of a step unless
    ///     explicitly specified.
    /// </summary>
    public string? View { get; }

    /// <summary>
    ///     The view model to return to the UI if this step is returning a view (optional)
    /// </summary>
    public object? ViewModel { get; }
}
