﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using Umbraco.Core;

namespace Umbraco.Tests.Benchmarks
{
    // some conclusions
    // - ActivatorCreateInstance is slow
    // - it's faster to get+invoke the ctor
    // - emitting the ctor is unless if invoked only 1

    // TODO: Check out https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.constructorbuilder?view=netcore-3.1 ?

    //[Config(typeof(Config))]
    [MemoryDiagnoser]
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
                    .WithIterationTime(TimeInterval.FromMilliseconds(400))
                    .WithWarmupCount(3)
                    .WithIterationCount(6));
            }
        }

        private readonly IFoo _foo = new Foo(null);
        private ConstructorInfo _ctorInfo;
        private Func<IFoo, IFoo> _dynamicMethod;
        private Func<IFoo, IFoo> _expressionMethod;
        private Func<IFoo, IFoo> _expressionMethod2;
        private Func<IFoo, IFoo> _expressionMethod3;
        private Func<IFoo, IFoo> _expressionMethod4;
        private Func<IFoo, IFoo> _emittedCtor;

        [GlobalSetup]
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

            // generate a dynamic method
            //
            // ldarg.0      // obj0
            // newobj instance void [Umbraco.Tests.Benchmarks]Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks / Foo::.ctor(class [Umbraco.Tests.Benchmarks] Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks/IFoo)
            // ret

            var meth = new DynamicMethod(string.Empty, typeof (IFoo), ctorArgTypes, type.Module, true);
            var gen = meth.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);
            _dynamicMethod = (Func<IFoo, IFoo>) meth.CreateDelegate(typeof (Func<IFoo, IFoo>));

            // generate a compiled expression
            //
            // ldarg.0      // content
            // newobj instance void [Umbraco.Tests.Benchmarks]Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks / Foo::.ctor(class [Umbraco.Tests.Benchmarks] Umbraco.Tests.Benchmarks.CtorInvokeBenchmarks/IFoo)
            // ret

            var exprArg = Expression.Parameter(typeof (IFoo), "content");
            var exprNew = Expression.New(constructor, exprArg);
            var expr = Expression.Lambda<Func<IFoo, IFoo>>(exprNew, exprArg);
            _expressionMethod = expr.Compile();

            // create a dynamic assembly
            // dump to disk so we can review IL code with eg DotPeek

            var assemblyName = new AssemblyName("Umbraco.Tests.Benchmarks.IL");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            var module = assembly.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");
            var typeBuilder = module.DefineType("CtorInvoke", TypeAttributes.Public | TypeAttributes.Abstract);

            var expressionMethodBuilder = typeBuilder.DefineMethod("ExpressionCtor",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (IFoo), ctorArgTypes);
            expr.CompileToMethod(expressionMethodBuilder);

            var dynamicMethodBuilder = typeBuilder.DefineMethod("DynamicCtor",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(IFoo), ctorArgTypes);
            gen = dynamicMethodBuilder.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);
            meth.CreateDelegate(typeof (Func<IFoo, IFoo>));

            var btype = typeBuilder.CreateType(); // need to build before saving
            assembly.Save("Umbraco.Tests.Benchmarks.IL.dll");

            // at that point,
            //   _dynamicMethod is 2x slower than direct ctor
            //   _expressionMethod is 6x slower than direct ctor
            // which is weird as inspecting the assembly IL shows that they are generated
            // exactly the same, and yet it is confirmed eg by https://stackoverflow.com/questions/4211418
            //
            // not sure why exactly
            // see https://stackoverflow.com/questions/13431573
            // see http://mattwarren.org/2017/01/25/How-do-.NET-delegates-work/#different-types-of-delegates
            //
            // note that all the benchmark methods have the very same IL code so it's
            // really the 'callvirt ...' that ends up doing different things
            //
            // more readings:
            // http://byterot.blogspot.dk/2012/05/performance-comparison-of-code.html
            // https://stackoverflow.com/questions/1296683
            // https://stackoverflow.com/questions/44239127
            // that last one points to
            // https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/
            // which reads ... "Expression.Compile creates a DynamicMethod and associates it with an anonymous assembly
            // to run it in a sandboxed environment. This makes it safe for a dynamic method to be emitted and executed
            // by partially trusted code but adds some run-time overhead."
            // and, turning things into a delegate (below) removes that overhead...

            // turning it into a delegate seems cool, _expressionMethod2 is ~ _dynamicMethod
            _expressionMethod2 = (Func<IFoo, IFoo>) Delegate.CreateDelegate(typeof (Func<IFoo, IFoo>), btype.GetMethod("ExpressionCtor"));

            // nope, this won't work, throws an ArgumentException because 'MethodInfo must be a MethodInfo object'
            // and here it's of type System.Reflection.Emit.DynamicMethod+RTDynamicMethod - whereas the btype one is ok
            // so, the dynamic assembly step is required
            //
            //_expressionMethod3 = (Func<IFoo, IFoo>) Delegate.CreateDelegate(typeof (Func<IFoo, IFoo>), _expressionMethod.Method);

            // but, our utilities know how to do it!
            _expressionMethod3 = ReflectionUtilities.CompileToDelegate(expr);
            _expressionMethod4 = ReflectionUtilities.GetCtor<Foo, IFoo>();

            // however, unfortunately, the generated "compiled to delegate" code cannot access private stuff :(

            _emittedCtor = ReflectionUtilities.EmitConstructor<Func<IFoo, Foo>>();
        }

        public IFoo IlCtor(IFoo foo)
        {
            return new Foo(foo);
        }

        [Benchmark(Baseline = true)]
        public void DirectCtor()
        {
            var foo = new Foo(_foo);
        }

        [Benchmark]
        public void EmitCtor()
        {
            var ctor = ReflectionUtilities.EmitConstructor<Func<IFoo, Foo>>();
            var foo = ctor(_foo);
        }

        [Benchmark]
        public void ActivatorCreateInstance()
        {
            var foo = Activator.CreateInstance(typeof(Foo), _foo);
        }

        [Benchmark]
        public void GetAndInvokeCtor()
        {
            var ctorArgTypes = new[] { typeof(IFoo) };
            var type = typeof(Foo);
            var ctorInfo = type.GetConstructor(ctorArgTypes);
            var foo = ctorInfo.Invoke(new object[] { _foo });
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
            var foo = _expressionMethod(_foo);
        }

        [Benchmark]
        public void Expression2Ctor()
        {
            var foo = _expressionMethod2(_foo);
        }

        [Benchmark]
        public void Expression3Ctor()
        {
            var foo = _expressionMethod3(_foo);
        }

        [Benchmark]
        public void Expression4Ctor()
        {
            var foo = _expressionMethod4(_foo);
        }

        [Benchmark]
        public void EmittedCtor()
        {
            var foo = _emittedCtor(_foo);
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
