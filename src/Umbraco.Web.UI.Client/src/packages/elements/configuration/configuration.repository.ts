import { _getCachedElementConfiguration } from './configuration.cache.js';
import { UmbElementConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbElementConfigurationModel } from './types.js';
import type { UmbContentConfigurationRepository } from '@umbraco-cms/backoffice/content';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

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
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbElementConfigurationModel>> {
		return _getCachedElementConfiguration(() => this.#serverDataSource.getConfiguration());
	}
}

export { UmbElementConfigurationRepository as api };
