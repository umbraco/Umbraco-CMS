﻿namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

public class DataTypeResponseModel : DataTypeModelBase
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
}
