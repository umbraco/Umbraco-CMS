using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using LightInject;

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
                CacheHelper.CreateDisabledCacheHelper().RuntimeCache,
                new ManifestValueValidatorCollection(Enumerable.Empty<IManifestValueValidator>()),
                Logger)
            {
                Path = TestHelper.CurrentAssemblyDirectory
            };
            Container.Register(_ => manifestBuilder);

            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;
            Container.GetInstance<DataEditorCollectionBuilder>()
                .Clear()
                .Add(typeListProducerList);
        }

        [Test]
        public void AssertConfigurationIsValid()
        {
            var profiles = Container.GetAllInstances<Profile>().ToArray();

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
