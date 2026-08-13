import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { TemporaryFileService } from '@umbraco-cms/backoffice/external/backend-api';
import type { TemporaryFileConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbApiResponse } from '@umbraco-cms/backoffice/resources';

export class UmbTemporaryFileConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the temporary file configuration.
	 * @returns {Promise<UmbApiResponse<TemporaryFileConfigurationResponseModel>>} The temporary file configuration
	 */
	getConfig(): Promise<
		UmbApiResponse<{ data: TemporaryFileConfigurationResponseModel; request: Request; response: Response }>
	> {
		return tryExecute(this.#host, TemporaryFileService.getTemporaryFileConfiguration(), { disableNotifications: true });
	}
}
