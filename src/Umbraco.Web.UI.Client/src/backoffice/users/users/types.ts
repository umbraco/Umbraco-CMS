import type {
	CreateUserRequestModel,
	CreateUserResponseModel,
	DirectionModel,
	DisableUserRequestModel,
	EnableUserRequestModel,
	InviteUserRequestModel,
	UpdateUserRequestModel,
	UserOrderModel,
	UserResponseModel,
	UserStateModel,
} from '@umbraco-cms/backoffice/backend-api';

import {
	DataSourceResponse,
	UmbDataSource,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
} from '@umbraco-cms/backoffice/repository';

export interface UmbCreateUserResponseModel {
	user: UserResponseModel;
	createData: CreateUserResponseModel;
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
	extends UmbDataSource<CreateUserRequestModel, CreateUserResponseModel, UpdateUserRequestModel, UserResponseModel> {
	invite(data: InviteUserRequestModel): Promise<any>;
	enable(data: EnableUserRequestModel): Promise<any>;
	disable(data: DisableUserRequestModel): Promise<any>;
}

export interface UmbUserSetGroupDataSource {
	setGroups(userIds: string[], userGroupIds: string[]): Promise<UmbDataSourceErrorResponse>;
}

export interface UmbUserDetailRepository
	extends UmbDetailRepository<
		CreateUserRequestModel,
		UmbCreateUserResponseModel,
		UpdateUserRequestModel,
		UserResponseModel
	> {
	invite(data: InviteUserRequestModel): Promise<any>;
}
