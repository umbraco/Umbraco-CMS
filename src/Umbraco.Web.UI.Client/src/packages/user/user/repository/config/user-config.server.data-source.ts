import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbUserConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the user configuration.
	 * @memberof UmbUserConfigServerDataSource
	 */
	getUserConfig() {
		return tryExecute(this.#host, UserService.getUserConfiguration());
	}

	/**
	 * Get the current user configuration.
	 * @memberof UmbUserConfigServerDataSource
	 */
	getCurrentUserConfig() {
		return tryExecute(this.#host, UserService.getUserCurrentConfiguration());
	}
}
