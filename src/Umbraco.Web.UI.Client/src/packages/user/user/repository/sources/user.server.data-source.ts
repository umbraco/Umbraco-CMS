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
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
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

	/**
	 * Gets the user with the given id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	async read(id: string) {
		if (!id) throw new Error('Id is missing');
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUserById({ id }));
		return extendDataSourceResponseData<UmbUserDetail>(response, {
			entityType: USER_ENTITY_TYPE,
		});
	}

	/**
	 * Creates a new user
	 * @param {CreateUserRequestModel} data
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	create(data: CreateUserRequestModel) {
		return tryExecuteAndNotify(this.#host, UserResource.postUser({ requestBody: data }));
	}

	/**
	 * Updates the user with the given id
	 * @param {string} id
	 * @param {UpdateUserRequestModel} data
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
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

	/**
	 * Deletes the user with the given id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserResource.deleteUserById({ id }));
	}

	/**
	 * Creates an avatar for the user with the given id based on a temporary uploaded file
	 * @param {string} id
	 * @param {string} fileId
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	createAvatar(id: string, fileId: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecuteAndNotify(this.#host, UserResource.postUserAvatarById({ id, requestBody: { fileId } }));
	}

	/**
	 * Deletes the avatar for the user with the given id
	 * @param {string} id
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecuteAndNotify(this.#host, UserResource.deleteUserAvatarById({ id }));
	}
}
