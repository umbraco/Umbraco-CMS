import type { UmbCurrentUserModel } from '../types.js';
import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the current user that fetches data from the server
 * @export
 * @class UmbCurrentUserServerDataSource
 */
export class UmbCurrentUserServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbCurrentUserServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbCurrentUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the current user
	 * @return {*}
	 * @memberof UmbCurrentUserServerDataSource
	 */
	async getCurrentUser() {
		const { data, error } = await tryExecuteAndNotify(this.#host, UserResource.getUserCurrent());

		if (data) {
			const user: UmbCurrentUserModel = {
				unique: data.id,
				email: data.email,
				userName: data.userName,
				name: data.name,
				languageIsoCode: data.languageIsoCode || 'en-us', // TODO: make global variable
				documentStartNodeIds: data.documentStartNodeIds,
				mediaStartNodeIds: data.mediaStartNodeIds,
				avatarUrls: data.avatarUrls,
				languages: data.languages,
				hasAccessToAllLanguages: data.hasAccessToAllLanguages,
				permissions: data.permissions,
			};
			return { data: user };
		}

		return { error };
	}
}
