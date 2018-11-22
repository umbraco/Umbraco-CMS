using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace umbraco.BusinessLogic.Utils
{

    /// <summary>
    /// A TextWriter class based on the StringWriter that can support any encoding, not just UTF-16
    /// as is the default of the normal StringWriter class
    /// </summary>
    public class EncodedStringWriter : StringWriter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EncodedStringWriter"/> class.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="enc">The enc.</param>
        public EncodedStringWriter(StringBuilder sb, Encoding enc)
            : base(sb)
        {
            m_encoding = enc;
        }

        private Encoding m_encoding;

        /// <summary>
        /// Gets the <see cref="T:System.Text.Encoding"></see> in which the output is written.
        /// </summary>
        /// <value></value>
        /// <returns>The Encoding in which the output is written.</returns>
        public override Encoding Encoding
        {
            get
            {
                return m_encoding;
            }
        }
    }
}
