namespace Umbraco.Cms.ManagementApi.Security;

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
