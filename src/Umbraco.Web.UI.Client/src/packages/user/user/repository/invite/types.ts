import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(data: InviteUserRequestModel): Promise<UmbDataSourceErrorResponse>;
}
