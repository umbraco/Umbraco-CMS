import { UmbElementConfigurationServerDataSource } from './configuration.server.data-source.js';
import type { UmbElementConfigurationModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @description - Repository for Element configuration.
 * @exports
 * @class UmbElementConfigurationRepository
 * @augments UmbRepositoryBase
 */
export class UmbElementConfigurationRepository extends UmbRepositoryBase {
	#serverDataSource = new UmbElementConfigurationServerDataSource(this);
	/**
	 * Requests the Element configuration
	 * @returns {Promise<UmbRepositoryResponse<UmbElementConfigurationModel>>} - The element configuration.
	 * @memberof UmbElementConfigurationRepository
	 */
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbElementConfigurationModel>> {
		return this.#serverDataSource.getConfiguration();
	}
}
