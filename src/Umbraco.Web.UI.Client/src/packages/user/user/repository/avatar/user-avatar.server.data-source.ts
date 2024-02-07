import type { SetAvatarRequestModel } from '@umbraco-cms/backoffice/backend-api';
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
	 * @param {string} unique
	 * @param {string} fileUnique
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	createAvatar(unique: string, fileUnique: string): Promise<UmbDataSourceErrorResponse> {
		const requestBody: SetAvatarRequestModel = {
			file: {
				id: fileUnique,
			},
		};

		return tryExecuteAndNotify(this.#host, UserResource.postUserAvatarById({ id: unique, requestBody }));
	}

	/**
	 * Deletes the avatar for the user with the given id
	 * @param {string} unique
	 * @return {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	deleteAvatar(unique: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecuteAndNotify(this.#host, UserResource.deleteUserAvatarById({ id: unique }));
	}
}
