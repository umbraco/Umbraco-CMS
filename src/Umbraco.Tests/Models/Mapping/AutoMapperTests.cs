using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class AutoMapperTests : UmbracoTestBase
    {
        protected override void Compose()
        {
            base.Compose();

            var manifestBuilder = new ManifestParser(
                CacheHelper.Disabled.RuntimeCache,
                new ManifestValueValidatorCollection(Enumerable.Empty<IManifestValueValidator>()),
                Composition.Logger)
            {
                Path = TestHelper.CurrentAssemblyDirectory
            };
            Composition.Register(_ => manifestBuilder);

            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;
            Composition.WithCollectionBuilder<DataEditorCollectionBuilder>()
                .Clear()
                .Add(typeListProducerList);
        }

        [Test]
        public void AssertConfigurationIsValid()
        {
            var profiles = Factory.GetAllInstances<Profile>().ToArray();

            var config = new MapperConfiguration(cfg =>
            {
                foreach (var profile in profiles)
                    cfg.AddProfile(profile);
            });

            // validate each profile (better granularity for error reports)

            Console.WriteLine("Validate each profile:");
            foreach (var profile in profiles)
            {
                try
                {
                    config.AssertConfigurationIsValid(profile.GetType().FullName);
                    //Console.WriteLine("OK " + profile.GetType().FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("KO " + profile.GetType().FullName);
                    Console.WriteLine(e);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Validate each profile and throw:");
            foreach (var profile in profiles)
            {
                try
                {
                    config.AssertConfigurationIsValid(profile.GetType().FullName);
                }
                catch
                {
                    Console.WriteLine("KO " + profile.GetType().FullName);
                    throw;
                }
            }

            // validate the global config
            Console.WriteLine();
            Console.WriteLine("Validate global config:");
            config.AssertConfigurationIsValid();
        }
    }
}
