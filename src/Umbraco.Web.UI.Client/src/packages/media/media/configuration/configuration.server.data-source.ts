import type { UmbMediaConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbMediaConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the media configuration from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbMediaConfigurationModel>>} - The media configuration.
	 * @memberof UmbMediaConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbMediaConfigurationModel>> {
		const { data, error } = await tryExecute(this, MediaService.getMediaConfiguration());

		if (data) {
			const mappedData: UmbMediaConfigurationModel = {
				disableDeleteWhenReferenced: data.disableDeleteWhenReferenced,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
