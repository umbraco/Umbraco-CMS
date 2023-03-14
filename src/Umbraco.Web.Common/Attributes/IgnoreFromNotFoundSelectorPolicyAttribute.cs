namespace Umbraco.Cms.Web.Common.Attributes;

/// <summary>
///     When applied to an api controller it will be routed to the /Umbraco/BackOffice prefix route so we can determine if
///     it
///     is a back office route or not.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IgnoreFromNotFoundSelectorPolicyAttribute : Attribute
{
}
