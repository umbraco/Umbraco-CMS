namespace Umbraco.Core.Sync
{
    public interface IDatabaseServerMessenger: IServerMessenger
    {
        void Sync();
    }
}
