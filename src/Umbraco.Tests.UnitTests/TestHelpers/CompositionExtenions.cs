using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Infrastructure.Composing;

namespace Umbraco.Tests.UnitTests.TestHelpers
{
    public static class CompositionExtenions
    {
        public static IFactory CreateFactory(this Composition composition)
        {
            composition.RegisterBuilders();
            return ServiceProviderFactoryAdapter.Wrap(composition.Services.BuildServiceProvider());
        }
    }
}
