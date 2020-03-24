using LightInject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Tests.Common.Composing;

namespace Umbraco.Tests.Common
{
    public class Assertions
    {
        public static void AssertContainer(ServiceContainer container, bool reportOnly = false)
        {
            var results = container.Validate().ToList();
            foreach (var resultGroup in results.GroupBy(x => x.Severity).OrderBy(x => x.Key))
            {
                Console.WriteLine($"{resultGroup.Key}: {resultGroup.Count()}");
            }

            foreach (var resultGroup in results.GroupBy(x => x.Severity).OrderBy(x => x.Key))
            {
                foreach (var result in resultGroup)
                {
                    Console.WriteLine();
                    Console.Write(result.ToText());
                }
            }   

            if (!reportOnly)
                Assert.AreEqual(0, results.Count);
        }

    }
}
