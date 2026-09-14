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
	 * @param {string} unique - The unique id of the user.
	 * @param {string} fileUnique - The unique id of the temporary uploaded file.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the create avatar operation.
	 * @memberof UmbUserAvatarServerDataSource
	 */
	createAvatar(unique: string, fileUnique: string): Promise<UmbDataSourceErrorResponse> {
		const body: SetAvatarRequestModel = {
			file: {
				id: fileUnique,
			},
		};

		return tryExecute(this.#host, UserService.postUserAvatarById({ path: { id: unique }, body }));
	}

	/**
	 * Deletes the avatar for the user with the given id
	 * @param {string} unique - The unique id of the user.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the delete avatar operation.
	 * @memberof UmbUserAvatarServerDataSource
	 */
	deleteAvatar(unique: string): Promise<UmbDataSourceErrorResponse> {
		return tryExecute(this.#host, UserService.deleteUserAvatarById({ path: { id: unique } }));
	}
}
