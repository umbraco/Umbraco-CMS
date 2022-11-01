namespace Umbraco.Cms.ManagementApi.Security;

public interface IClientSecretManager
{
    string Get(string clientId);
}
