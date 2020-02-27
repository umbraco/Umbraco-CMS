namespace Umbraco.Net
{
    public interface IUmbracoApplicationLifetime
    {
        bool IsRestarting { get; }
        void Restart();
    }
}
