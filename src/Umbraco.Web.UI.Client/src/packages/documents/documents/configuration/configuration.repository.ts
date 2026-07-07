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
	/**
	 * The cached document configuration, shared across all instances.
	 */
	static #configuration: Promise<UmbRepositoryResponse<UmbDocumentConfigurationModel>>;

	readonly #serverDataSource = new UmbDocumentConfigurationServerDataSource(this);

	/**
	 * Requests the Document configuration from the server, or returns the cached configuration if it has already been fetched.
	 * @returns {Promise<UmbRepositoryResponse<UmbDocumentConfigurationModel>>} - The document configuration.
	 * @memberof UmbDocumentConfigurationRepository
	 */
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbDocumentConfigurationModel>> {
		return (UmbDocumentConfigurationRepository.#configuration ??= this.#serverDataSource.getConfiguration());
	}
}

export { UmbDocumentConfigurationRepository as api };
