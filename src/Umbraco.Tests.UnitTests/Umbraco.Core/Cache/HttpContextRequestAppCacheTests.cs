// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using static Umbraco.Cms.Core.Cache.HttpContextRequestAppCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache
{
    [TestFixture]
    public class HttpContextRequestAppCacheTests : AppCacheTests
    {
        private HttpContextRequestAppCache _appCache;
        private IHttpContextAccessor _httpContextAccessor;

        public override void Setup()
        {
            base.Setup();
            var httpContext = new DefaultHttpContext();

            var services = new ServiceCollection();
            services.AddScoped<RequestLock>();
            var serviceProviderFactory = new DefaultServiceProviderFactory();            
            IServiceCollection builder = serviceProviderFactory.CreateBuilder(services);
            IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(builder);
            httpContext.RequestServices = serviceProvider;

            _httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
            _appCache = new HttpContextRequestAppCache(_httpContextAccessor);
        }

        internal override IAppCache AppCache => _appCache;

        protected override int GetTotalItemCount => _httpContextAccessor.HttpContext.Items.Count;
    }
}
