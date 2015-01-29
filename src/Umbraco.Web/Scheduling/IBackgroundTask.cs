using System;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    internal interface IBackgroundTask : IDisposable
    {
        void Run();
        Task RunAsync();
        bool IsAsync { get; }
    }
}