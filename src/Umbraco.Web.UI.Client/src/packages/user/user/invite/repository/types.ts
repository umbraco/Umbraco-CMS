import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(requestModel: InviteUserRequestModel): Promise<UmbDataSourceResponse<UmbUserDetail>>;
	resendInvite(userId: string, requestModel: any): Promise<UmbDataSourceResponse<UmbUserDetail>>;
}
