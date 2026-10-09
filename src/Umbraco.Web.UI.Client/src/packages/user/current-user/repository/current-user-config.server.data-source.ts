import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { CurrentUserConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbCurrentUserConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the current user configuration.
	 * @returns {Promise<UmbDataSourceResponse<CurrentUserConfigurationResponseModel>>} The current user configuration, or an error
	 * @memberof UmbCurrentUserConfigServerDataSource
	 */
	getCurrentUserConfig(): Promise<UmbDataSourceResponse<CurrentUserConfigurationResponseModel>> {
		return tryExecute(this.#host, UserService.getUserCurrentConfiguration());
	}
}
