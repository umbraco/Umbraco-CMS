import { UmbDataTypesConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbDataTypesConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The cached configuration of data types, shared across all repository instances.
 */
let configurationPromise: Promise<UmbRepositoryResponse<UmbDataTypesConfigurationModel>> | undefined;

/**
 * @description - Repository for the configuration of data types.
 * @exports
 * @class UmbDataTypesConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbDataTypesConfigurationRepository extends UmbRepositoryBase {
	readonly #serverDataSource = new UmbDataTypesConfigurationServerDataSource(this);

	/**
	 * Requests the configuration of data types from the server, or returns the cached configuration if it has already been fetched. Error responses are not cached.
	 * @returns {Promise<UmbRepositoryResponse<UmbDataTypesConfigurationModel>>} - The configuration of data types.
	 * @memberof UmbDataTypesConfigurationRepository
	 */
	async requestConfiguration(): Promise<UmbRepositoryResponse<UmbDataTypesConfigurationModel>> {
		configurationPromise ??= this.#serverDataSource.getConfiguration();
		const response = await configurationPromise;
		if (response.error) {
			configurationPromise = undefined;
		}
		return response;
	}
}

export { UmbDataTypesConfigurationRepository as api };

/**
 * Test-only.
 * @internal
 */
export function resetUmbDataTypesConfigurationCache(): void {
	configurationPromise = undefined;
}
