import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(requestModel: InviteUserRequestModel): Promise<UmbDataSourceErrorResponse>;
	resendInvite(userId: string, requestModel: any): Promise<UmbDataSourceErrorResponse>;
}
