import type { UmbDataTypesConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDataTypesConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the configuration of data types from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbDataTypesConfigurationModel>>} - The configuration of data types.
	 * @memberof UmbDataTypesConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbDataTypesConfigurationModel>> {
		const { data, error } = await tryExecute(this, DataTypeService.getDataTypeConfiguration());

		if (data) {
			const mappedData: UmbDataTypesConfigurationModel = {
				showDeprecatedPropertyEditors: data.showDeprecatedPropertyEditors,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
