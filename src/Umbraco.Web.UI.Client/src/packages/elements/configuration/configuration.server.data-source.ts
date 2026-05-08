import type { UmbElementConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbElementConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the element configuration from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbElementConfigurationModel>>} - The element configuration.
	 * @memberof UmbElementConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbElementConfigurationModel>> {
		const { data, error } = await tryExecute(this, ElementService.getElementConfiguration());

		if (data) {
			const mappedData: UmbElementConfigurationModel = {
				disableDeleteWhenReferenced: data.disableDeleteWhenReferenced,
				disableUnpublishWhenReferenced: data.disableUnpublishWhenReferenced,
				allowEditInvariantFromNonDefault: data.allowEditInvariantFromNonDefault,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
