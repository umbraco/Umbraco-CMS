﻿using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models.Sections;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class SectionMapperProfile : Profile
    {
        public SectionMapperProfile(ILocalizedTextService textService)
        {
            CreateMap<ISection, Section>()
                .ForMember(dest => dest.RoutePath, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => textService.Localize("sections/" + src.Alias, (IDictionary<string, string>)null)))
                .ReverseMap(); //backwards too!
        }
    }
}
