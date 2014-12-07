using System;

namespace Umbraco.Web.Scheduling
{
    internal interface IBackgroundTask : IDisposable
    {
        void Run();
    }
}