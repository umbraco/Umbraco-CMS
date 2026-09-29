import { UmbMediaConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbMediaConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The cached media configuration, shared across all repository instances.
 */
let configurationPromise: Promise<UmbRepositoryResponse<UmbMediaConfigurationModel>> | undefined;

/**
 * @description - Repository for Media configuration.
 * @exports
 * @class UmbMediaConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbMediaConfigurationRepository extends UmbRepositoryBase {
	readonly #serverDataSource = new UmbMediaConfigurationServerDataSource(this);

	/**
	 * Requests the Media configuration from the server, or returns the cached configuration if it has already been fetched. Error responses are not cached.
	 * @returns {Promise<UmbRepositoryResponse<UmbMediaConfigurationModel>>} - The media configuration.
	 * @memberof UmbMediaConfigurationRepository
	 */
	async requestConfiguration(): Promise<UmbRepositoryResponse<UmbMediaConfigurationModel>> {
		configurationPromise ??= this.#serverDataSource.getConfiguration();
		const response = await configurationPromise;
		if (response.error) {
			configurationPromise = undefined;
		}
		return response;
	}
}

export { UmbMediaConfigurationRepository as api };

/**
 * Test-only.
 * @internal
 */
export function resetUmbMediaConfigurationCache(): void {
	configurationPromise = undefined;
}
