using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Implements a json read converter for <see cref="IAccessRule" />.
/// </summary>
internal class DashboardAccessRuleConverter : JsonReadConverter<IAccessRule>
{
    /// <inheritdoc />
    protected override IAccessRule Create(Type objectType, string path, JObject jObject) => new AccessRule();

    /// <inheritdoc />
    protected override void Deserialize(JObject jobject, IAccessRule target, JsonSerializer serializer)
    {
        // see Create above, target is either DataEditor (parameter) or ConfiguredDataEditor (property)
        if (!(target is AccessRule accessRule))
        {
            throw new PanicException("panic.");
        }

        GetRule(accessRule, jobject, "grant", AccessRuleType.Grant);
        GetRule(accessRule, jobject, "deny", AccessRuleType.Deny);
        GetRule(accessRule, jobject, "grantBySection", AccessRuleType.GrantBySection);

        if (accessRule.Type == AccessRuleType.Unknown)
        {
            throw new InvalidOperationException("Rule is not defined.");
        }
    }

    private void GetRule(AccessRule rule, JObject jobject, string name, AccessRuleType type)
    {
        JToken? token = jobject[name];
        if (token == null)
        {
            return;
        }

        if (rule.Type != AccessRuleType.Unknown)
        {
            throw new InvalidOperationException("Multiple definition of a rule.");
        }

        if (token.Type != JTokenType.String)
        {
            throw new InvalidOperationException("Rule value is not a string.");
        }

        rule.Type = type;
        rule.Value = token.Value<string>();
    }
}
