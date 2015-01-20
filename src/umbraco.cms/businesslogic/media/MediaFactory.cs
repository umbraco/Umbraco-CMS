using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace umbraco.cms.businesslogic.media
{

    public class PostedMediaFile
    {
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public Stream InputStream { get; set; }
        public bool ReplaceExisting { get; set; }

        public void SaveAs(string filename)
        {
            using (var s = new FileStream(filename, FileMode.Create))
            {
                try
                {
                    int readCount;
                    var buffer = new byte[8192];

                    if (InputStream.CanSeek)
                        InputStream.Seek(0, SeekOrigin.Begin);

                    while ((readCount = InputStream.Read(buffer, 0, buffer.Length)) != 0)
                        s.Write(buffer, 0, readCount);

                    s.Flush();
                }
                finally
                {
                    s.Close();
                }
            }
            
        }
    }
}
