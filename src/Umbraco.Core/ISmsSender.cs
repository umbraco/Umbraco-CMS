using System.Threading.Tasks;

namespace Umbraco.Core
{
    /// <summary>
    /// Service to send an SMS
    /// </summary>
    public interface ISmsSender
    {
        // borrowed from https://github.com/dotnet/AspNetCore.Docs/blob/master/aspnetcore/common/samples/WebApplication1/Services/ISmsSender.cs#L8

        Task SendSmsAsync(string number, string message);
    }
}
