using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Http.Clients;

namespace Umbraco.Core.Http
{
    public class LightInjectHttpClientFactoryComposer : ICoreComposer, IDisposable
    {
        protected ServiceCollection _seviceCollection;
        protected ServiceProvider _serviceProvider;
        private bool _disposedValue;

        public LightInjectHttpClientFactoryComposer()
        {
            _seviceCollection = new ServiceCollection();
        }
        public void Compose(Composition composition)
        {
            AddHttpClients();

            _serviceProvider = _seviceCollection.BuildServiceProvider();

            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            composition.Register<IHttpClientFactory>(container => httpClientFactory, Lifetime.Singleton);
        }

        protected void AddHttpClients()
        {
            _seviceCollection.AddHttpClient();
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.RestApiInstallUrl);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.RestApiUpgradeChecklUrl);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.RestApiBaseUrl);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.RemoteDashboardUrl);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.RemoteXmlUrl);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.OurUmbracoHelpPage);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.OurUmbracoWebApi);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.ShopUmbracoBase);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.EmbedProviderBase);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.KeepAlivePing);
            _seviceCollection.AddHttpClient(Constants.HttpClientConstants.ReportSiteTask);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _serviceProvider?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
