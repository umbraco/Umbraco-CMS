﻿namespace Umbraco.Cms.Core.Security.OperationStatus;

public enum BackOfficeUserClientCredentialsOperationStatus
{
    Success,
    DuplicateClientId,
    ReservedClientId,
    InvalidUser
}
