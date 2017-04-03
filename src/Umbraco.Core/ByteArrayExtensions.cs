namespace Umbraco.Core
{
    public static class ByteArrayExtensions
    {
        private static readonly char[] BytesToHexStringLookup = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string ToHexString(this byte[] bytes)
        {
            int i = 0, p = 0, bytesLength = bytes.Length;
            var chars = new char[bytesLength * 2];
            while (i < bytesLength)
            {
                var b = bytes[i++];
                chars[p++] = BytesToHexStringLookup[b / 0x10];
                chars[p++] = BytesToHexStringLookup[b % 0x10];
            }
            return new string(chars, 0, chars.Length);
        }

        public static string ToHexString(this byte[] bytes, char separator, int blockSize, int blockCount)
        {
            int p = 0, bytesLength = bytes.Length, count = 0, size = 0;
            var chars = new char[bytesLength * 2 + blockCount];
            for (var i = 0; i < bytesLength; i++)
            {
                var b = bytes[i++];
                chars[p++] = BytesToHexStringLookup[b / 0x10];
                chars[p++] = BytesToHexStringLookup[b % 0x10];
                if (count == blockCount) continue;
                if (++size < blockSize) continue;

                chars[p++] = '/';
                size = 0;
                count++;
            }
            return new string(chars, 0, chars.Length);
        }
    }
}
