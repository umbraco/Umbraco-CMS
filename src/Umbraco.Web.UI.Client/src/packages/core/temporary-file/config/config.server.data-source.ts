import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { TemporaryFileService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbTemporaryFileConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get the temporary file configuration.
	 */
	getConfig() {
		return tryExecute(this.#host, TemporaryFileService.getTemporaryFileConfiguration());
	}
}
