import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbUserAvatarServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates an avatar for the user with the given id based on a temporary uploaded file
	 * @param {string} id
	 * @param {string} fileId
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	createAvatar(id: string, fileId: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecuteAndNotify(this.#host, UserResource.postUserAvatarById({ id, requestBody: { file: fileId } }));
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
