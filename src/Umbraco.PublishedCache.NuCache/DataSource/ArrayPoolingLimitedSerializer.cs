using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CSharpTest.Net.Serialization;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource
{
    internal class ArrayPoolingLimitedSerializer
    {
        private readonly StringPool _internPool = new StringPool();

        public string? ReadString(Stream stream, bool intern = false)
        {
            unchecked
            {
                int sz = VariantNumberSerializer.Int32.ReadFrom(stream);
                if (sz == 0)
                {
                    return string.Empty;
                }

                if (sz == int.MinValue)
                {
                    return null;
                }

                if (!(sz >= 0 && sz <= int.MaxValue))
                {
                    throw new InvalidDataException();
                }

                char[]? chars = null;
                try
                {
                    chars = ArrayPool<char>.Shared.Rent(sz);
                    for (int i = 0; i < sz; i++)
                    {
                        chars[i] = (char)VariantNumberSerializer.Int32.ReadFrom(stream);
                    }

                    Span<char> str = chars.AsSpan()[..sz];

                    if (intern)
                    {
                        return _internPool.GetOrAdd(str);
                    }

                    return StringPool.Shared.GetOrAdd(str);
                }
                finally
                {
                    if (chars != null)
                    {
                        ArrayPool<char>.Shared.Return(chars, true);
                    }
                }
            }
        }

#pragma warning disable IDE0032 // Use auto property
        private static ArrayPoolingLimitedSerializer _stringSerializer = new ArrayPoolingLimitedSerializer();
#pragma warning restore IDE0032 // Use auto property

        internal static ArrayPoolingLimitedSerializer StringSerializer { get => _stringSerializer; set => _stringSerializer = value; }
    }
}
