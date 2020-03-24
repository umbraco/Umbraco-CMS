using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Tests.Integration
{
    /// <summary>
    /// Ensures the host lifetime ends as soon as code execution is done
    /// </summary>
    public class TestLifetime : IHostLifetime
    {
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

   
}
