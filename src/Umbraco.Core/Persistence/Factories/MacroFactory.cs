using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MacroFactory : IEntityFactory<Macro, MacroDto>
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public Macro BuildEntity(MacroDto dto)
        {
            var model = new Macro(dto.Id, dto.UseInEditor, dto.RefreshRate, dto.Alias, dto.Name, dto.ScriptType, dto.ScriptAssembly, dto.Xslt, dto.CacheByPage, dto.CachePersonalized, dto.DontRender, dto.Python);
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            model.ResetDirtyProperties(false);
            return model;
        }

        public MacroDto BuildDto(Macro entity)
        {
            var dto = new MacroDto()
                {
                    Alias = entity.Alias,
                    CacheByPage = entity.CacheByPage,
                    CachePersonalized = entity.CachePersonalized,
                    DontRender = entity.DontRender,
                    Name = entity.Name,
                    Python = entity.ScriptPath,
                    RefreshRate = entity.RefreshRate,
                    ScriptAssembly = entity.ControlAssembly,
                    ScriptType = entity.ControlType,
                    UseInEditor = entity.UseInEditor,
                    Xslt = entity.XsltPath
                };
            if (entity.HasIdentity)
                dto.Id = short.Parse(entity.Id.ToString(CultureInfo.InvariantCulture));

            return dto;
        }

        #endregion
    }
}