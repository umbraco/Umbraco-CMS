using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.HealthChecks;

[DataContract(Name = "healthCheckAction", Namespace = "")]
public class HealthCheckAction
{
    /// <summary>
    ///     The name of the action - this is used to name the fix button
    /// </summary>
    [DataMember(Name = "name")]
    private string? _name;

    /// <summary>
    ///     Empty ctor used for serialization
    /// </summary>
    public HealthCheckAction()
    {
    }

    /// <summary>
    ///     Default ctor
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="healthCheckId"></param>
    public HealthCheckAction(string alias, Guid healthCheckId)
    {
        Alias = alias;
        HealthCheckId = healthCheckId;
    }

    /// <summary>
    ///     The alias of the action - this is used by the Health Check instance to execute the action
    /// </summary>
    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    /// <summary>
    ///     The Id of the Health Check instance
    /// </summary>
    /// <remarks>
    ///     This is used to find the Health Check instance to execute this action
    /// </remarks>
    [DataMember(Name = "healthCheckId")]
    public Guid? HealthCheckId { get; set; }

    /// <summary>
    ///     This could be used if the status has a custom view that specifies some parameters to be sent to the server
    ///     when an action needs to be executed
    /// </summary>
    [DataMember(Name = "actionParameters")]
    public Dictionary<string, object>? ActionParameters { get; set; }

    public string? Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     The description of the action - this is used to give a description before executing the action
    /// </summary>
    [DataMember(Name = "description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Indicates if a value is required to rectify the issue
    /// </summary>
    [DataMember(Name = "valueRequired")]
    public bool ValueRequired { get; set; }

    /// <summary>
    ///     Indicates if a value required, how it is validated
    /// </summary>
    [DataMember(Name = "providedValueValidation")]
    public string? ProvidedValueValidation { get; set; }

    /// <summary>
    ///     Indicates if a value required, and is validated by a regex, what the regex to use is
    /// </summary>
    [DataMember(Name = "providedValueValidationRegex")]
    public string? ProvidedValueValidationRegex { get; set; }

    /// <summary>
    ///     Provides a value to rectify the issue
    /// </summary>
    [DataMember(Name = "providedValue")]
    public string? ProvidedValue { get; set; }
}
