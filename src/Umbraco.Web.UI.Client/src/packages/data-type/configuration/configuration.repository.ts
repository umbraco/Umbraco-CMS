import { _getCachedDataTypeConfiguration } from './configuration.cache.js';
import { UmbDataTypeConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbDataTypeConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @description - Repository for Data Type configuration.
 * @exports
 * @class UmbDataTypeConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbDataTypeConfigurationRepository extends UmbRepositoryBase {
	readonly #serverDataSource = new UmbDataTypeConfigurationServerDataSource(this);

	/**
	 * Requests the Data Type configuration from the server, or returns the cached configuration if it has already been fetched. Error responses are not cached.
	 * @returns {Promise<UmbRepositoryResponse<UmbDataTypeConfigurationModel>>} - The data type configuration.
	 * @memberof UmbDataTypeConfigurationRepository
	 */
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbDataTypeConfigurationModel>> {
		return _getCachedDataTypeConfiguration(() => this.#serverDataSource.getConfiguration());
	}
}

export { UmbDataTypeConfigurationRepository as api };
