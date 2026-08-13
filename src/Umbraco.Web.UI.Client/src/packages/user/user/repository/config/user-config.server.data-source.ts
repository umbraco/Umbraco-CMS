import type { UmbUserConfigurationModel, UmbCurrentUserConfigurationModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbUserConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the user configuration.
	 * @returns {Promise<UmbDataSourceResponse<UmbUserConfigurationModel>>} The user configuration.
	 * @memberof UmbUserConfigServerDataSource
	 */
	getUserConfig(): Promise<UmbDataSourceResponse<UmbUserConfigurationModel>> {
		return tryExecute(this.#host, UserService.getUserConfiguration());
	}

	/**
	 * Get the current user configuration.
	 * @returns {Promise<UmbDataSourceResponse<UmbCurrentUserConfigurationModel>>} The current user configuration.
	 * @memberof UmbUserConfigServerDataSource
	 */
	getCurrentUserConfig(): Promise<UmbDataSourceResponse<UmbCurrentUserConfigurationModel>> {
		return tryExecute(this.#host, UserService.getUserCurrentConfiguration());
	}
}
