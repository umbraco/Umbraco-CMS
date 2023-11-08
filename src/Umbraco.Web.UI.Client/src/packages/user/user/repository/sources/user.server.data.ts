import { USER_ENTITY_TYPE, UmbUserDetail, UmbUserDetailDataSource } from '../../types.js';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	extendDataSourceResponseData,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	UpdateUserRequestModel,
	UserPresentationBaseModel,
	UserResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserServerDataSource implements UmbUserDetailDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	// Details
	createScaffold(parentId: string | null): Promise<DataSourceResponse<UserPresentationBaseModel>> {
		throw new Error('Method not implemented.');
	}

	async get(id: string) {
		if (!id) throw new Error('Id is missing');
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUserById({ id }));
		return extendDataSourceResponseData<UmbUserDetail>(response, {
			entityType: USER_ENTITY_TYPE,
		});
	}

	insert(data: CreateUserRequestModel) {
		return tryExecuteAndNotify(this.#host, UserResource.postUser({ requestBody: data }));
	}

	update(id: string, data: UpdateUserRequestModel) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.putUserById({
				id,
				requestBody: data,
			}),
		);
	}

	delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserResource.deleteUserById({ id }));
	}

	uploadAvatar(id: string, file: File): Promise<UmbDataSourceErrorResponse> {
		throw new Error('Method not implemented.');
	}

	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse> {
		throw new Error('Method not implemented.');
	}
}
