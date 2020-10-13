using BenchmarkDotNet.Attributes;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class DefaultShortStringHelperBenchmark
    {
        DefaultShortStringHelper helper;
        const string input = "0123 中文测试 中文测试 léger ZÔRG (2) a?? *x";
        public DefaultShortStringHelperBenchmark()
        {
            var settings = SettingsForTests.GenerateMockUmbracoSettings();
            var contentMock = Mock.Get(settings.RequestHandler);
            contentMock.Setup(x => x.CharCollection).Returns(Enumerable.Empty<IChar>());
            contentMock.Setup(x => x.ConvertUrlsToAscii).Returns(false);

            helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(settings));

        }
        [Benchmark]
        public void CleanStringDefaultConfig()
        {
            var alias = helper.CleanStringForSafeAlias(input);
            var filename = helper.CleanStringForSafeFileName(input);
            var segment = helper.CleanStringForUrlSegment(input);
        }


        //Char[]
        // * Summary *

        //        BenchmarkDotNet = v0.11.3, OS = Windows 10.0.18362
        //Intel Core i5-8265U CPU 1.60GHz(Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
        // [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0
        //  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0


        //                   Method |     Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        //------------------------- |---------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
        // CleanStringDefaultConfig | 5.839 us | 0.0319 us | 0.0298 us |      0.8850 |           - |           - |             2.73 KB |

        //// * Legends *
        //  Mean                : Arithmetic mean of all measurements
        //  Error               : Half of 99.9% confidence interval
        //  StdDev              : Standard deviation of all measurements
        //  Gen 0/1k Op         : GC Generation 0 collects per 1k Operations
        //  Gen 1/1k Op         : GC Generation 1 collects per 1k Operations
        //  Gen 2/1k Op         : GC Generation 2 collects per 1k Operations
        //  Allocated Memory/Op : Allocated memory per single operation(managed only, inclusive, 1KB = 1024B)
        //  1 us                : 1 Microsecond(0.000001 sec)

        //// * Diagnostic Output - MemoryDiagnoser *

        //        Span<T>
        // * Summary *

//        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.18362
//Intel Core i5-8265U CPU 1.60GHz(Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
// [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0
//  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0


//                   Method |     Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
//------------------------- |---------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
// CleanStringDefaultConfig | 7.144 us | 0.2048 us | 0.2192 us |      0.6790 |           - |           - |             2.11 KB |


    }
}
