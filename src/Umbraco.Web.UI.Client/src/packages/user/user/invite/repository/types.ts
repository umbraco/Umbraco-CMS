import type { UmbUserDetailModel } from '../../types.js';
import type { InviteUserRequestModel, ResendInviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(requestModel: InviteUserRequestModel): Promise<DataSourceResponse<UmbUserDetailModel>>;
	resendInvite(requestModel: ResendInviteUserRequestModel): Promise<UmbDataSourceErrorResponse>;
}
