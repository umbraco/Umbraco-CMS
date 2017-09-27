using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Clr
{
    [TestFixture]
    public class ReflectionUtilitiesTests
    {
        [Test]
        public void Test()
        {
            // cannot ctor a private class
            var ctor1 = ReflectionUtilities.GetCtor<PrivateClass>();
            Assert.Throws<MethodAccessException>(() => _ = ctor1());

            // cannot private ctor a public class
            var ctor2 = ReflectionUtilities.GetCtor<PublicClass, int>();
            Assert.Throws<MethodAccessException>(() => _ = ctor2(0));

            // can public ctor a public class
            var ctor3 = ReflectionUtilities.GetCtor<PublicClass, string>();
            Assert.IsNotNull(ctor3(string.Empty));

            // works if not a dynamic assembly
            var ctor4 = ReflectionUtilities.GetCtor<PrivateClass>(false);
            Assert.IsNotNull(ctor4());

            // we need the dynasm flag everywhere, because in some cases we do not
            // want to create a dynasm that will stay around - eg when using the
            // generated stuff only a few times

            // collectible assemblies - created with RunAndCollect...
            // https://msdn.microsoft.com/en-us/library/dd554932(v=vs.100).aspx
            // with restrictions, but we could try it?

            // so...
            // GetCtor<PrivateClass>(ReflectionUtilities.Compile.None)

            // should we find a way for a dynamic assembly to access private stuff?
        }

        [Test]
        public void SingleDynAsmTest()
        {
            var expr1 = ReflectionUtilities.GetCtorExpr<PrivateClass>();
            //var ctor2 = ReflectionUtilities.GetCtorExpr<PublicClass, int>();
            //var ctor3 = ReflectionUtilities.GetCtorExpr<PublicClass, string>();

            // creates one single dynamic assembly containing all methods
            var ctors = ReflectionUtilities.CompileToDelegates(expr1);
            var ctor1 = ctors[0];

            // still, cannot access private stuff
            Assert.Throws<MethodAccessException>(() => _ = ctor1());
        }

        [Test]
        public void MoreTest()
        {
            // can get a ctor via generic and compile
            var expr1 = ReflectionUtilities.GetCtorExpr<Class1>();
            var ctor1 = ReflectionUtilities.Compile(expr1);
            Assert.IsInstanceOf<Class1>(ctor1());

            // direct
            var ctor1A = ReflectionUtilities.GetCtor<Class1>();
            Assert.IsInstanceOf<Class1>(ctor1A());

            // can get a ctor via type and compile
            var expr2 = ReflectionUtilities.GetCtorExpr<Func<Class1>>(typeof (Class1));
            var ctor2 = ReflectionUtilities.Compile(expr2);
            Assert.IsInstanceOf<Class1>(ctor2());

            // direct
            var ctor2A = ReflectionUtilities.GetCtor<Func<Class1>>(typeof (Class1));
            Assert.IsInstanceOf<Class1>(ctor2A());

            // if type is unknown (in a variable)
            var ctor2B = ReflectionUtilities.GetCtor<Func<object>>(typeof (Class1));
            Assert.IsInstanceOf<Class1>(ctor2B());

            // cannot get a ctor for a private class
            var ctorP1 = ReflectionUtilities.GetCtor<Class1P>();
            Assert.Throws<MethodAccessException>(() => ctorP1());

            // unless we don't compile to an assembly
            var ctorP1A = ReflectionUtilities.GetCtor<Class1P>(access: ReflectionUtilities.NoAssembly);
            Assert.IsInstanceOf<Class1P>(ctorP1A());

            // so...
            // if I create a dynamic method delegate by writing IL it's fast and I can access private stuff
            // if I create an expression it's easier than IL but it's slower
            // if I compile the expression, the speed is same as delegate but then I cannot access private stuff
            // so ... what should I do?!
        }

        // todo
        // - figure out the private/public thing
        // - implement the dynasm enumeration
        // - figure out ctors in model factory - ie the casting thing

        // this is how we could indicate what to do?
        private enum DynAsm
        {
            None,
            Run,
            RunAndCollect
        }

        private class PrivateClass { }

        public class PublicClass
        {
            private PublicClass(int i) { }

            public PublicClass(string s) { }
        }

        public class Class1 { }

        public class Class2 { }

        private class Class1P { }
    }
}
