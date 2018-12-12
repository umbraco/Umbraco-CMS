using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Composing;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class StandaloneTests
    {
        [Test]
        public void Test()
        {
            // this is almost what CoreRuntime does, without
            // - managing MainDom
            // - configuring for unhandled exceptions, assembly resolution, application root path
            // - testing for database, and for upgrades (runtime level)
            // - assigning the factory to Current.Factory

            // create the very basic and essential things we need
            var logger = new ConsoleLogger();
            var profiler = Mock.Of<IProfiler>();
            var profilingLogger = new ProfilingLogger(logger, profiler);
            var appCaches = CacheHelper.Disabled;
            var databaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
            var typeLoader = new TypeLoader(appCaches.RuntimeCache, LocalTempStorage.Default, profilingLogger);
            var runtimeState = Mock.Of<IRuntimeState>();
            Mock.Get(runtimeState).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            // create the register and the composition
            var register = RegisterFactory.Create();
            var composition = new Composition(register, typeLoader, profilingLogger, runtimeState);
            composition.RegisterEssentials(logger, profiler, profilingLogger, appCaches, databaseFactory, typeLoader, runtimeState);

            // create the core runtime and have it compose itself
            var coreRuntime = new CoreRuntime();
            coreRuntime.Compose(composition);

            // fixme
            // at that point, CoreRuntime also does
            //composition.RegisterUnique(mainDom)
            // we should make it
            //composition.RegisterUnique<IMainDom>(mainDom)
            // because some components want to use it
            // (and then, what would a standalone maindom be?)

            // get the components
            // all of them?
            var componentTypes = typeLoader.GetTypes<IUmbracoComponent>();
            // filtered?
            //var componentTypes = typeLoader.GetTypes<IUmbracoComponent>()
            //    .Where(x => !x.FullName.StartsWith("Umbraco.Web"));
            // single?
            //var componentTypes = new[] { typeof(CoreRuntimeComponent) };
            var components = new Core.Components.Components(composition, componentTypes, profilingLogger);

            // get components to compose themselves
            components.Compose();

            // create the factory
            var factory = composition.CreateFactory();

            // at that point Umbraco is fully composed
            // but nothing is initialized (no maindom, nothing - beware!)
            // to actually *run* Umbraco standalone, better use a StandaloneRuntime
            // that would inherit from CoreRuntime and ensure everything starts

            // get components to initialize themselves
            //components.Initialize(factory);

            // and then, validate
            var lightInjectContainer = (LightInject.ServiceContainer) factory.Concrete;
            var results = lightInjectContainer.Validate().ToList();
            foreach (var resultGroup in results.GroupBy(x => x.Severity))
            foreach (var result in resultGroup)
            {
                Console.WriteLine();
                Console.WriteLine($"{result.Severity}: {WordWrap(result.Message, 120)}");
                var target = result.ValidationTarget;
                Console.Write("\t");
                Console.Write(target.ServiceName);
                Console.Write(target.DeclaringService.ServiceType);
                if (!target.DeclaringService.ServiceName.IsNullOrWhiteSpace())
                {
                    Console.Write(" '");
                    Console.Write(target.DeclaringService.ServiceName);
                    Console.Write("'");
                }

                Console.Write("     ");
                if (target.DeclaringService.Lifetime == null)
                    Console.Write("?");
                else
                    Console.Write(target.DeclaringService.Lifetime.ToString().TrimStart("LightInject."));
                Console.WriteLine();
                Console.Write("\t");
                Console.Write(target.Parameter);
                Console.WriteLine();
            }
            Assert.AreEqual(0, results.Count);
        }

        public static string WordWrap(string text, int width)
        {
            int pos, next;
            var sb = new StringBuilder();
            var nl = Environment.NewLine;

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                var eol = text.IndexOf(nl, pos, StringComparison.Ordinal);

                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + nl.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        var len = eol - pos;

                        if (len > width)
                            len = BreakLine(text, pos, width);

                        if (pos > 0)
                            sb.Append("\t\t");
                        sb.Append(text, pos, len);
                        sb.Append(nl);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && char.IsWhiteSpace(text[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(nl); // Empty line
            }

            return sb.ToString();
        }

        public static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}
