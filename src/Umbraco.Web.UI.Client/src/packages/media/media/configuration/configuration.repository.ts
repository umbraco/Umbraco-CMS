import { _getCachedMediaConfiguration } from './configuration.cache.js';
import { UmbMediaConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbMediaConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

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
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbMediaConfigurationModel>> {
		return _getCachedMediaConfiguration(() => this.#serverDataSource.getConfiguration());
	}
}

export { UmbMediaConfigurationRepository as api };
