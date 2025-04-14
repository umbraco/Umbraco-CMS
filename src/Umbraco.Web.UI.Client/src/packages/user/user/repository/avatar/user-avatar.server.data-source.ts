import type { SetAvatarRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbUserAvatarServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates an avatar for the user with the given id based on a temporary uploaded file
	 * @param {string} unique
	 * @param {string} fileUnique
	 * @returns {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	createAvatar(unique: string, fileUnique: string): Promise<UmbDataSourceErrorResponse> {
		const requestBody: SetAvatarRequestModel = {
			file: {
				id: fileUnique,
			},
		};

		return tryExecute(this.#host, UserService.postUserAvatarById({ id: unique, requestBody }));
	}

	/**
	 * Deletes the avatar for the user with the given id
	 * @param {string} unique
	 * @returns {*}  {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserServerDataSource
	 */
	deleteAvatar(unique: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecute(this.#host, UserService.deleteUserAvatarById({ id: unique }));
	}
}
