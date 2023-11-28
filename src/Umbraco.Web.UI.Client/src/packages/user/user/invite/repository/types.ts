import { UmbUserDetailModel } from '../../types.js';
import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(requestModel: InviteUserRequestModel): Promise<DataSourceResponse<UmbUserDetailModel>>;
	resendInvite(userId: string, requestModel: any): Promise<DataSourceResponse<UmbUserDetailModel>>;
}
