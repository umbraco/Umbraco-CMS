﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Collections;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class ConcurrentDictionaryBenchmarks
    {
        private static readonly ConcurrentDictionary<CompositeTypeTypeKey, bool> AssignableTypeCache = new ConcurrentDictionary<CompositeTypeTypeKey, bool>();

        private static object input = new Bar();

        private static Type source = typeof(Bar);

        private static Type target = typeof(Foo);

        [Benchmark(Baseline = true)]
        public bool GetCachedCanAssignFactory()
        {
            return AssignableTypeCache.GetOrAdd(new CompositeTypeTypeKey(source, target), k =>
            {
                var ksource = k.Type1;
                var ktarget = k.Type2;

                return ktarget.IsAssignableFrom(ksource) && typeof(IConvertible).IsAssignableFrom(ksource);
            });
        }

        [Benchmark]
        public bool GetCachedCanAssignNoFactory()
        {
            // This method is 10% faster
            var key = new CompositeTypeTypeKey(source, target);
            bool canConvert;
            if (AssignableTypeCache.TryGetValue(key, out canConvert))
            {
                return canConvert;
            }

            // "is" is faster than "IsAssignableFrom"
            if (input is IConvertible && target.IsAssignableFrom(source))
            {
                return AssignableTypeCache[key] = true;
            }

            return AssignableTypeCache[key] = false;
        }

        private class Foo : IConvertible
        {
            public TypeCode GetTypeCode()
            {
                return TypeCode.Object;
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                if (conversionType == typeof(Foo))
                {
                    return new Foo();
                }

                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }

        private class Bar : Foo
        {

        }
    }
}
