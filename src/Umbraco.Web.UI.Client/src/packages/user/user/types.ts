import type {
	CreateUserRequestModel,
	CreateUserResponseModel,
	DirectionModel,
	UpdateUserRequestModel,
	UserOrderModel,
	UserResponseModel,
	UserStateModel,
} from '@umbraco-cms/backoffice/backend-api';

import { UmbDataSource, UmbDataSourceErrorResponse, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export type UmbUserDetail = UserResponseModel & {
	entityType: 'user';
};

export interface UmbCreateUserResponseModel {
	user: UserResponseModel;
	initialPassword: CreateUserResponseModel['initialPassword'];
}

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
	extends UmbDataSource<CreateUserRequestModel, CreateUserResponseModel, UpdateUserRequestModel, UmbUserDetail> {}

export interface UmbUserSetGroupDataSource {
	setGroups(userIds: string[], userGroupIds: string[]): Promise<UmbDataSourceErrorResponse>;
}

export interface UmbUserUnlockDataSource {
	unlock(userIds: string[]): Promise<UmbDataSourceErrorResponse>;
}

export interface UmbUserDetailRepository
	extends UmbDetailRepository<
		CreateUserRequestModel,
		UmbCreateUserResponseModel,
		UpdateUserRequestModel,
		UserResponseModel
	> {}
