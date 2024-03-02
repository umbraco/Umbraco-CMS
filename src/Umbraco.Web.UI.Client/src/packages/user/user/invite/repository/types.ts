import type { UmbUserDetailModel } from '../../types.js';
import type { UmbDataSourceResponse, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbInviteUserDataSource {
	invite(requestModel: UmbInviteUserRequestModel): Promise<UmbDataSourceResponse<UmbUserDetailModel>>;
	resendInvite(requestModel: UmbResendUserInviteRequestModel): Promise<UmbDataSourceErrorResponse>;
}

export interface UmbInviteUserRequestModel {
	email: string;
	userName: string;
	name: string;
	userGroupUniques: Array<string>;
	message: string | null;
}

export interface UmbResendUserInviteRequestModel {
	user: { unique: string };
	message: string | null;
}
