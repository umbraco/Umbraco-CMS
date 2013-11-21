using System;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using System.Text;
using Umbraco.Core.Models.Rdbms;
using System.Runtime.CompilerServices;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.cms.businesslogic.media;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    public partial class TestRepositoryAbstractionLayer
    {
        internal MacroRepository Macro { get { return new MacroRepository(this); } }

        internal class MacroRepository : EntityRepository
        {
            internal MacroRepository(TestRepositoryAbstractionLayer tral) : base(tral) { }

            public int CountAll
            {
                get
                {
                    return _r.ExecuteScalar<int>("select count(id) from cmsMacro");
                }
            }

            public int CountAllProperties(int macroId)
            {
                return _r.ExecuteScalar<int>("select count(id) from cmsMacroProperty where [macro] = @0", macroId);
            }

            public int CountAllPropertyTypes
            {
                get
                {
                    return _r.ExecuteScalar<int>("select count(id) from cmsMacroPropertyType");
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroPropertyDto CreateTestMacroProperty(int macroId, int macroPropertyTypeId, string propertyName = TEST_MACRO_PROPERTY_NAME, string propertyAlias = TEST_MACRO_PROPERTY_ALIAS)
            {
                _r.Execute("insert into [cmsMacroProperty] (macroPropertyHidden, macroPropertyType, macro, macroPropertySortOrder, macroPropertyAlias, macroPropertyName) " +
                                           " values (@macroPropertyHidden, @macroPropertyType, @macro, @macroPropertySortOrder, @macroPropertyAlias, @macroPropertyName) ",
                                           new
                                           {
                                               macroPropertyHidden = true,
                                               macroPropertyType = macroPropertyTypeId,
                                               macro = macroId,
                                               macroPropertySortOrder = 0,
                                               macroPropertyAlias = propertyName + uniqueAliasSuffix,
                                               macroPropertyName = propertyAlias + uniqueNameSuffix
                                           });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacroProperty]");
                return _tral.GetDto<MacroPropertyDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroDto CreateMacro(string name, string alias)
            {
                _r.Execute("insert into [cmsMacro] " +
                     " (macroUseInEditor, macroRefreshRate, macroAlias, macroName, macroScriptType, " +
                     " macroScriptAssembly, macroXSLT, macroPython, macroDontRender, macroCacheByPage, macroCachePersonalized) " +
                     " VALUES  " +
                     " (@macroUseInEditor, @macroRefreshRate, @macroAlias, @macroName, @macroScriptType, " +
                     " @macroScriptAssembly, @macroXSLT, @macroPython, @macroDontRender, @macroCacheByPage, @macroCachePersonalized) ",
                     new
                     {
                         macroUseInEditor = true,
                         macroRefreshRate = 0,
                         macroAlias = alias, // "twitterAds",
                         macroName = name, //"Twitter Ads",
                         macroScriptType = "mst",
                         macroScriptAssembly = "",
                         macroXSLT = "some.xslt",
                         macroPython = "",
                         macroDontRender = false,
                         macroCacheByPage = true,
                         macroCachePersonalized = false
                     });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacro]");
                return _tral.GetDto<MacroDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroPropertyTypeDto CreateMacroPropertyType(string alias)
            {
                _r.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +
                                          " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                          new
                                          {
                                              macroPropertyTypeAlias = alias,
                                              macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                              macroPropertyTypeRenderType = "context",
                                              macroPropertyTypeBaseType = "string"
                                          });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
                return _tral.GetDto<MacroPropertyTypeDto>(id);
            }

            public void DeleteMacro(int macroId)
            {
                _r.Execute("delete from  [cmsMacro] where id = @0", macroId);
            }
            public void DeleteMacroProperty(int macroPropertyId)
            {
                _r.Execute("delete from  [cmsMacroProperty] where id = @0", macroPropertyId);
            }
            public void DeleteMacroPropertyType(int macroPropertyTypeId)
            {
                _r.Execute("delete from  [cmsMacroPropertyType] where id = @0", macroPropertyTypeId);
            }



        }

    }
}
