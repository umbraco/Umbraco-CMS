namespace Umbraco.Cms.Core.OAuth
{
    public interface ISecretEncryptor
    {
        string Encrypt(string value);

        string Decrypt(string value);
    }
}
