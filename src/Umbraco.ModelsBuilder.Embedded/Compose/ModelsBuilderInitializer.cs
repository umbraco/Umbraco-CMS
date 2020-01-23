using System.Web;
using System.Web.Compilation;
using Umbraco.ModelsBuilder.Embedded.Compose;

[assembly: PreApplicationStartMethod(typeof(ModelsBuilderInitializer), "Initialize")]

namespace Umbraco.ModelsBuilder.Embedded.Compose
{
    public static class ModelsBuilderInitializer
    {
        public static void Initialize()
        {
            // for some reason, netstandard is missing from BuildManager.ReferencedAssemblies and yet, is part of
            // the references that CSharpCompiler receives - in some cases eg when building views - but not when
            // using BuildManager to build the PureLive models - where is it coming from? cannot figure it out

            // so... cheating here

            // this is equivalent to adding
            //         <add assembly="netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" />
            // to web.config system.web/compilation/assemblies

            var netStandard = ReferencedAssemblies.GetNetStandardAssembly();
            if (netStandard != null)
                BuildManager.AddReferencedAssembly(netStandard);
        }
    }
}
