// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Perfolizer.Horology;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    internal sealed class NavNode
    {
        public Guid Key;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public Guid? Parent;
    }

    internal sealed class Tree
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public readonly ConcurrentDictionary<Guid, NavNode> Structure = new();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public readonly Dictionary<Guid, bool> Published = new();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public readonly List<Guid> Candidates = new();

        // Mirror of ContentNavigationServiceBase.TryGetAncestorsKeysFromStructure
        public bool TryGetAncestorsKeys(Guid childKey, out List<Guid> ancestorsKeys)
        {
            var ancestors = new List<Guid>();
            if (Structure.TryGetValue(childKey, out NavNode? node) is false)
            {
                ancestorsKeys = ancestors;
                return false;
            }

            while (node!.Parent is not null && Structure.TryGetValue(node.Parent.Value, out node))
            {
                ancestors.Add(node!.Key);
            }

            ancestorsKeys = ancestors;
            return true;
        }

        public bool TryGetParentKey(Guid childKey, out Guid? parentKey)
        {
            if (Structure.TryGetValue(childKey, out NavNode? childNode))
            {
                parentKey = childNode.Parent;
                return true;
            }

            // Child doesn't exist
            parentKey = null;
            return false;
        }

        // Mirror of DocumentPublishStatusService.IsPublished
        public bool IsPublished(Guid key) => Published.TryGetValue(key, out var p) && p;
    }

    [Config(typeof(InProcessStableRunConfig))]
    public class FilterAvailableBenchmarks
    {
        private sealed class InProcessStableRunConfig : ManualConfig
        {
            public InProcessStableRunConfig()
            {
                AddJob(Job.Default
                    .WithLaunchCount(1)
                    .WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                    .WithWarmupCount(5)
                    .WithIterationCount(10)
                    .WithToolchain(InProcessEmitToolchain.Instance));
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }

        // Breadth x Depth combinations chosen to represent realistic renders:
        //   (10, 3)  ~ 1,110 nodes   — a large section / nav render
        //   (5, 6)   ~ 19,530 nodes  — a big descendants()/sitemap query
        //   (8, 5)   ~ 37,448 nodes  — deep + wide worst case
        [Params(3, 5, 6)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public int Depth;

        [Params(5, 10)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        public int Breadth;

        private Tree _tree = null!;

        [GlobalSetup]
        public void Setup()
        {
            _tree = BuildTree(Depth, Breadth);

            var baseline = FilterAvailable_Baseline();
            var fixedResult = FilterAvailable_Memoised();
            if (baseline.Count != fixedResult.Count || !baseline.SequenceEqual(fixedResult))
            {
                throw new InvalidOperationException(
                    $"Result mismatch: baseline={baseline.Count}, fix={fixedResult.Count}. " +
                    "The memoised implementation is not equivalent — do not trust the numbers.");
            }

            Console.WriteLine(
                $"[setup] Depth={Depth} Breadth={Breadth} " +
                $"nodes={_tree.Structure.Count} candidates={_tree.Candidates.Count} " +
                $"survivors={baseline.Count}");
        }

        private static Tree BuildTree(int depth, int breadth)
        {
            var tree = new Tree();
            var rng = new Random(12345); // fixed seed

            var root = new NavNode { Key = Guid.NewGuid(), Parent = null };
            tree.Structure[root.Key] = root;
            tree.Published[root.Key] = true;

            var current = new List<Guid> { root.Key };
            for (var level = 0; level < depth; level++)
            {
                var next = new List<Guid>(current.Count * breadth);
                foreach (var parent in current)
                {
                    for (var b = 0; b < breadth; b++)
                    {
                        var node = new NavNode { Key = Guid.NewGuid(), Parent = parent };
                        tree.Structure[node.Key] = node;
                        // ~3% unpublished, sprinkled through the tree.
                        tree.Published[node.Key] = rng.Next(100) >= 3;
                        next.Add(node.Key);
                        tree.Candidates.Add(node.Key);
                    }
                }
                current = next;
            }

            return tree;
        }

        [BenchmarkCategory("FilterAvailable"), Benchmark(Baseline = true)]
        public List<Guid> FilterAvailable_Baseline()
        {
            var result = new List<Guid>(_tree.Candidates.Count);
            foreach (var key in _tree.Candidates)
            {
                if (_tree.IsPublished(key) && HasPublishedAncestorPath_Baseline(key))
                {
                    result.Add(key);
                }
            }
            return result;
        }

        // Naive per-candidate baseline: re-walk the full ancestor chain for every key.
        private bool HasPublishedAncestorPath_Baseline(Guid key)
        {
            if (_tree.TryGetAncestorsKeys(key, out List<Guid> ancestors) is false)
            {
                return false;
            }

            foreach (var ancestor in ancestors)
            {
                if (_tree.IsPublished(ancestor) is false)
                {
                    return false;
                }
            }
            return true;
        }

        [BenchmarkCategory("FilterAvailable"), Benchmark]
        public List<Guid> FilterAvailable_Memoised()
            => WhereAncestorPathPublished(_tree.Candidates.Where(key => _tree.IsPublished(key)), culture: null).ToList();

        public IEnumerable<Guid> WhereAncestorPathPublished(IEnumerable<Guid> contentKeys, string? culture)
        {
            // memo[key] answers "are ALL ancestors of this key published (in the requested culture)?",
            // so the ancestor walk for a whole branch is done once instead of per candidate.
            var memo = new Dictionary<Guid, bool>();

            bool AncestorsPublished(Guid key)
            {
                if (memo.TryGetValue(key, out var cached))
                {
                    return cached;
                }

                bool result;
                if (_tree.TryGetParentKey(key, out Guid? parentKey) is false)
                {
                    // Node not in navigation - treat as having no published ancestor path.
                    result = false;
                }
                else if (parentKey is null)
                {
                    // Root: no ancestors, so the ancestor path is vacuously published.
                    result = true;
                }
                else
                {
                    bool parentPublished = _tree.IsPublished(parentKey.Value);
                    result = parentPublished && AncestorsPublished(parentKey.Value);
                }

                memo[key] = result;
                return result;
            }

            foreach (Guid key in contentKeys)
            {
                if (_tree.TryGetParentKey(key, out Guid? parentKey) is false)
                {
                    // Node not in navigation - treat as having no published ancestor path.
                    continue;
                }

                if (parentKey is null)
                {
                    // Root: no ancestors, so the ancestor path is vacuously published.
                    yield return key;
                    continue;
                }

                bool parentPublished = _tree.IsPublished(parentKey.Value);

                if (parentPublished && AncestorsPublished(parentKey.Value))
                {
                    yield return key;
                }
            }
        }
    }
}
