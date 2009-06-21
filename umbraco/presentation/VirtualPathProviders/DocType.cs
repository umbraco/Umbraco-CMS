using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Caching;
using System.IO;
using umbraco.Linq.DTMetal.CodeBuilder;
using umbraco.Linq.DTMetal.Engine;
using System.Collections;

namespace umbraco.presentation.VirtualPathProviders
{
    public class DocTypeProvider : VirtualPathProvider
    {
        private bool IsPathVirtual(string virtualPath)
        {
            String checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/vpp", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            if (IsPathVirtual(virtualPath))
            {
                var fileName = VirtualPathUtility.GetFileName(virtualPath);
                return fileName == "umbraco.DocTypes.dll";
            }
            else
            {
                return Previous.FileExists(virtualPath);
            }
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsPathVirtual(virtualPath))
            {
                if (FileExists(virtualPath))
                {
                    return new DocTypeVirtualFile(virtualPath, this);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Previous.GetFile(virtualPath);
            }
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {

            if (IsPathVirtual(virtualPath))
            {
                return null;
            }
            else
            {
                return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }
        }
    }

    public class DocTypeVirtualFile : VirtualFile
    {
        private DocTypeProvider _vpp;

        public DocTypeVirtualFile(string virtualPath, DocTypeProvider provider)
            : base(virtualPath)
        {
            _vpp = provider;
        }
        
        public override Stream Open()
        {
            MemoryStream stream = new MemoryStream();
            string dataContextName = "Umbraco";
            var generator = new DTMLGenerator(GlobalSettings.DbDSN, dataContextName, false);
            var dtml = generator.GenerateDTMLStream();
            var cb = ClassGenerator.CreateBuilder("Umbraco", GenerationLanguage.CSharp, dtml.DocTypeMarkupLanguage);
            cb.GenerateCode();
            cb.Save(stream);

            return new MemoryStream(stream.GetBuffer());
        }
    }
}
