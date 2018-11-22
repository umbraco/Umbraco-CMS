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
	//TODO: This class needs to be changed to use the new MultipleResolverBase, doing this will require migrating and cleaning up
	// a bunch of types so I have left it existing here under legacy code for now. 


    public class MediaFactory
    {
        internal static readonly List<IMediaFactory> Factories = new List<IMediaFactory>();

        static MediaFactory()
        {
            Initialize();
        }

        private static void Initialize()
        {
        	Factories.AddRange(
        		PluginManager.Current.CreateInstances<IMediaFactory>(
        			PluginManager.Current.ResolveMediaFactories()));        	
         
			Factories.Sort((f1, f2) => f1.Priority.CompareTo(f2.Priority));
        }

        public static IMediaFactory GetMediaFactory(int parentId, PostedMediaFile postedFile, User user)
        {
            var ext = Path.GetExtension(postedFile.FileName);
            if (ext == null)
                return null;

            var factories = Factories.Where(mf => mf.Extensions.Contains(ext.ToLower().TrimStart('.'))).ToList();

            if (factories.Count == 0)
                factories = Factories.Where(mf => mf.Extensions.Contains("*")).ToList();

            if (factories.Count > 0)
                return factories.FirstOrDefault(factory => factory.CanHandleMedia(parentId, postedFile, user));

            return null;
        }
    }

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
