import type {
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbDataSourceErrorResponse, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export interface IUmbUserDetailRepository
	extends UmbDetailRepository<
		CreateUserRequestModel,
		CreateUserResponseModel,
		UpdateUserRequestModel,
		UserResponseModel
	> {
	uploadAvatar(id: string, file: File): Promise<UmbDataSourceErrorResponse>;
	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse>;
}
