import { UmbElementConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbElementConfigurationModel } from './types.js';
import type { UmbContentConfigurationRepository } from '@umbraco-cms/backoffice/content';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The cached element configuration, shared across all repository instances.
 */
let configurationPromise: Promise<UmbRepositoryResponse<UmbElementConfigurationModel>> | undefined;

/**
 * @description - Repository for Element configuration.
 * @exports
 * @class UmbElementConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbElementConfigurationRepository extends UmbRepositoryBase implements UmbContentConfigurationRepository {
	readonly #serverDataSource = new UmbElementConfigurationServerDataSource(this);

	/**
	 * Requests the Element configuration from the server, or returns the cached configuration if it has already been fetched. Error responses are not cached.
	 * @returns {Promise<UmbRepositoryResponse<UmbElementConfigurationModel>>} - The element configuration.
	 * @memberof UmbElementConfigurationRepository
	 */
	async requestConfiguration(): Promise<UmbRepositoryResponse<UmbElementConfigurationModel>> {
		configurationPromise ??= this.#serverDataSource.getConfiguration();
		const response = await configurationPromise;
		if (response.error) {
			configurationPromise = undefined;
		}
		return response;
	}
}

export { UmbElementConfigurationRepository as api };

/**
 * Test-only.
 * @internal
 */
export function resetUmbElementConfigurationCache(): void {
	configurationPromise = undefined;
}
