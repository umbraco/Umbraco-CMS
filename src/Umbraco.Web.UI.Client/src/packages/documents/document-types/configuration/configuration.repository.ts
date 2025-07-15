import { UmbDocumentTypeConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbDocumentTypeConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @description - Repository for Document Type configuration.
 * @exports
 * @class UmbDocumentTypeConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbDocumentTypeConfigurationRepository extends UmbRepositoryBase {
	#serverDataSource = new UmbDocumentTypeConfigurationServerDataSource(this);

	/**
	 * Requests the Document Type configuration
	 * @returns {Promise<UmbRepositoryResponse<UmbDocumentTypeConfigurationModel>>} - The document type configuration.
	 * @memberof UmbDocumentTypeConfigurationRepository
	 */
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbDocumentTypeConfigurationModel>> {
		return this.#serverDataSource.getConfiguration();
	}
}
