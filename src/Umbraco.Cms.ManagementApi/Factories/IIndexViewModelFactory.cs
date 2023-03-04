﻿using Umbraco.Cms.ManagementApi.ViewModels.Search;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IIndexViewModelFactory
{
    IndexViewModel Create(string index);
}
