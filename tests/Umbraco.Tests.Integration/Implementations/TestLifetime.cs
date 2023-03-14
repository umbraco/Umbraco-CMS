// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.Implementations;

/// <summary>
///     Ensures the host lifetime ends as soon as code execution is done
/// </summary>
public class TestLifetime : IHostLifetime
{
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
