using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Exceptions;

namespace Umbraco.Web.Common.Middleware
{
    /// <summary>
    /// Executes when Umbraco booting fails in order to show the problem
    /// </summary>
    public class BootFailedMiddleware : IMiddleware
    {
        private readonly IRuntimeState _runtimeState;

        public BootFailedMiddleware(IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_runtimeState.Level == RuntimeLevel.BootFailed)
            {
                // short circuit
                BootFailedException.Rethrow(_runtimeState.BootFailedException);
            }
            else
            {
                await next(context);
            }
        }
    }
}
