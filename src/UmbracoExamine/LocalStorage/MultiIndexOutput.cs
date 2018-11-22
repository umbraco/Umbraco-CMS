using System;
using System.Linq;
using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    public class MultiIndexOutput : IndexOutput
    {
        private readonly IndexOutput[] _outputs;

        public MultiIndexOutput(params IndexOutput[] outputs)
        {
            if (outputs.Length < 1)
            {
                throw new InvalidOperationException("There must be at least one output specified");
            }
            _outputs = outputs;
        }

        public override void WriteByte(byte b)
        {
            foreach (var output in _outputs)
            {
                output.WriteByte(b);
            }
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            foreach (var output in _outputs)
            {
                output.WriteBytes(b, offset, length);
            }
        }

        public override void Flush()
        {
            foreach (var output in _outputs)
            {
                output.Flush();
            }
        }

        public override void Close()
        {
            foreach (var output in _outputs)
            {
                output.Close();
            }
        }

        public override long GetFilePointer()
        {
            //return the first
            return _outputs.First().GetFilePointer();
        }

        public override void Seek(long pos)
        {
            foreach (var output in _outputs)
            {
                output.Seek(pos);
            }
        }

        public override long Length()
        {
            //return the first
            return _outputs.First().GetFilePointer();
        }
    }
}