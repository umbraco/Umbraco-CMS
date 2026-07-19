// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class SampledSizeEstimatorTests
{
    [Test]
    public void Can_Return_Zero_For_Empty_Collection()
        => Assert.That(SampledSizeEstimator.Estimate(0, Array.Empty<int>(), _ => 10), Is.EqualTo(0));

    [Test]
    public void Can_Sum_Sizes_When_Count_Within_Sample()
        => Assert.That(SampledSizeEstimator.Estimate(3, new[] { 1, 2, 3 }, _ => 10), Is.EqualTo(30));

    [Test]
    public void Can_Extrapolate_From_Sample_When_Count_Exceeds_Sample()
    {
        // 10 items each sized 5, but only 2 are sampled → average 5 → 10 * 5 = 50.
        long result = SampledSizeEstimator.Estimate(10, Enumerable.Repeat(0, 10), _ => 5L, maxSample: 2);

        Assert.That(result, Is.EqualTo(50));
    }

    [Test]
    public void Can_Stop_Sizing_At_Sample_Cap()
    {
        var calls = 0;

        SampledSizeEstimator.Estimate(
            100,
            Enumerable.Range(0, 100),
            _ =>
            {
                calls++;
                return 1;
            },
            maxSample: 5);

        Assert.That(calls, Is.EqualTo(5));
    }
}
