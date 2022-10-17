namespace Umbraco.Cms.ManagementApi.Authorization;

public class ClientSecretManager : IClientSecretManager
{
    private Dictionary<string, string> _secretsByClientId = new();

    public string Get(string clientId)
    {
        if (_secretsByClientId.ContainsKey(clientId) == false)
        {
            _secretsByClientId[clientId] = Guid.NewGuid().ToString("N");
        }

        return _secretsByClientId[clientId];
    }
}

public interface IClientSecretManager
{
    string Get(string clientId);
}
