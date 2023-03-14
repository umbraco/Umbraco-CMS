using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Install.Models;

/// <summary>
///     Model to give to the front-end to collect the information for each step
/// </summary>
[DataContract(Name = "step", Namespace = "")]
[Obsolete("Will be replaced with IInstallStep in the new backoffice API")]
public abstract class InstallSetupStep<T> : InstallSetupStep
{
    /// <summary>
    ///     Defines the step model type on the server side so we can bind it
    /// </summary>
    [IgnoreDataMember]
    public override Type StepType => typeof(T);

    /// <summary>
    ///     The step execution method
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public abstract Task<InstallSetupResult?> ExecuteAsync(T model);

    /// <summary>
    ///     Determines if this step needs to execute based on the current state of the application and/or install process
    /// </summary>
    /// <returns></returns>
    public abstract bool RequiresExecution(T model);
}

[DataContract(Name = "step", Namespace = "")]
[Obsolete("Will be replaced with IInstallStep in the new backoffice API")]
public abstract class InstallSetupStep
{
    protected InstallSetupStep()
    {
        InstallSetupStepAttribute? att = GetType().GetCustomAttribute<InstallSetupStepAttribute>(false);
        if (att == null)
        {
            throw new InvalidOperationException("Each step must be attributed");
        }

        Name = att.Name;
        View = att.View;
        ServerOrder = att.ServerOrder;
        Description = att.Description;
        InstallTypeTarget = att.InstallTypeTarget;
        PerformsAppRestart = att.PerformsAppRestart;
    }

    [DataMember(Name = "name")]
    public string Name { get; private set; }

    [DataMember(Name = "view")]
    public virtual string View { get; private set; }

    /// <summary>
    ///     The view model used to render the view, by default is null but can be populated
    /// </summary>
    [DataMember(Name = "model")]
    public virtual object? ViewModel { get; private set; }

    [DataMember(Name = "description")]
    public string Description { get; private set; }

    [IgnoreDataMember]
    public InstallationType InstallTypeTarget { get; }

    [IgnoreDataMember]
    public bool PerformsAppRestart { get; }

    /// <summary>
    ///     Defines what order this step needs to execute on the server side since the
    ///     steps might be shown out of order on the front-end
    /// </summary>
    [DataMember(Name = "serverOrder")]
    public int ServerOrder { get; private set; }

    /// <summary>
    ///     Defines the step model type on the server side so we can bind it
    /// </summary>
    [IgnoreDataMember]
    public abstract Type StepType { get; }
}
