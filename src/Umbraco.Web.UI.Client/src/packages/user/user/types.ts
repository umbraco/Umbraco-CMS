import type {
	CreateUserRequestModel,
	CreateUserResponseModel,
	DirectionModel,
	UpdateUserRequestModel,
	UserOrderModel,
	UserResponseModel,
	UserStateModel,
} from '@umbraco-cms/backoffice/backend-api';

import { UmbDataSource, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export const USER_ENTITY_TYPE = 'user';

export type UmbUserDetail = UserResponseModel & {
	entityType: 'user';
};

export interface UmbUserCollectionFilterModel {
	skip?: number;
	take?: number;
	orderBy?: UserOrderModel;
	orderDirection?: DirectionModel;
	userGroupIds?: string[];
	userStates?: UserStateModel[];
	filter?: string;
}

export interface UmbUserDetailDataSource
	extends UmbDataSource<CreateUserRequestModel, CreateUserResponseModel, UpdateUserRequestModel, UmbUserDetail> {
	uploadAvatar(id: string, file: File): Promise<UmbDataSourceErrorResponse>;
	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse>;
}

export interface UmbUserSetGroupDataSource {
	setGroups(userIds: string[], userGroupIds: string[]): Promise<UmbDataSourceErrorResponse>;
}
