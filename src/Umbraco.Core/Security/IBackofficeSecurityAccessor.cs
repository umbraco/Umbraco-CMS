namespace Umbraco.Cms.Core.Security;

public interface IBackOfficeSecurityAccessor
{
    IBackOfficeSecurity? BackOfficeSecurity { get; }
}
