import { _getCachedDocumentConfiguration } from './configuration.cache.js';
import { UmbDocumentConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbDocumentConfigurationModel } from './types.js';
import type { UmbContentConfigurationRepository } from '@umbraco-cms/backoffice/content';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @description - Repository for Document configuration.
 * @exports
 * @class UmbDocumentConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbDocumentConfigurationRepository extends UmbRepositoryBase implements UmbContentConfigurationRepository {
	readonly #serverDataSource = new UmbDocumentConfigurationServerDataSource(this);

	/**
	 * Requests the Document configuration from the server, or returns the cached configuration if it has already been fetched. Error responses are not cached.
	 * @returns {Promise<UmbRepositoryResponse<UmbDocumentConfigurationModel>>} - The document configuration.
	 * @memberof UmbDocumentConfigurationRepository
	 */
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbDocumentConfigurationModel>> {
		return _getCachedDocumentConfiguration(() => this.#serverDataSource.getConfiguration());
	}
}

export { UmbDocumentConfigurationRepository as api };
