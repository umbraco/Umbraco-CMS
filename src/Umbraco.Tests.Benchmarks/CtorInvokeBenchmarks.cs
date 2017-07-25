using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;

namespace Umbraco.Tests.Benchmarks
{
    [Config(typeof(Config))]
    public class CtorInvokeBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(new MemoryDiagnoser());
                //Add(ExecutionValidator.FailOnError);

                //The 'quick and dirty' settings, so it runs a little quicker
                // see benchmarkdotnet FAQ
                Add(Job.Default
                    .WithLaunchCount(1) // benchmark process will be launched only once
                    .WithIterationTime(TimeInterval.FromMilliseconds(100)) // 100ms per iteration
                    .WithWarmupCount(3) // 3 warmup iteration
                    .WithTargetCount(3)); // 3 target iteration
            }
        }

        private ConstructorInfo _ctorInfo;
        private Func<IFoo, IFoo> _dynamicMethod;
        private Func<IFoo, IFoo> _expression;
        private IFoo _foo = new Foo(null);

        [Setup]
        public void Setup()
        {
            var ctorArgTypes = new[] { typeof(IFoo) };
            var type = typeof (Foo);
            var constructor = _ctorInfo = type.GetConstructor(ctorArgTypes);

            if (constructor == null)
                throw new Exception("Failed to get the ctor.");

            //IL_0000: ldarg.0      // this
            //IL_0001: ldfld        class Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks/IFoo Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks::_foo
            //IL_0006: newobj instance void Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks/Foo::.ctor(class Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks/IFoo)
            //IL_000b: pop
            //IL_000c: ret

            var meth = new DynamicMethod(string.Empty, typeof(Foo), ctorArgTypes, type.Module, true);
            var gen = meth.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            //gen.Emit(OpCodes.Call, constructor);
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);
            _dynamicMethod = (Func<IFoo, IFoo>) meth.CreateDelegate(typeof(Func<IFoo, IFoo>));

            var exprArg = Expression.Parameter(typeof(IFoo), "content");
            var exprNew = Expression.New(constructor, exprArg);
            var expr = Expression.Lambda<Func<IFoo, IFoo>>(exprNew, exprArg);
            _expression = expr.Compile();
        }

        public IFoo IlCtor(IFoo foo)
        {
            return new Foo(foo);
        }

        [Benchmark]
        public void DirectCtor()
        {
            var foo = new Foo(_foo);
        }

        [Benchmark]
        public void InvokeCtor()
        {
            var foo = _ctorInfo.Invoke(new object[] { _foo });
        }

        [Benchmark]
        public void DynamicMethodCtor()
        {
            var foo = _dynamicMethod(_foo);
        }

        [Benchmark]
        public void ExpressionCtor()
        {
            var foo = _expression(_foo);
        }

        public interface IFoo
        { }

        public class Foo : IFoo
        {
            public Foo(IFoo foo)
            { }
        }
    }
}
