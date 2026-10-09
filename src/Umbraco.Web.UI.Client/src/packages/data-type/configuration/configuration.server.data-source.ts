import type { UmbDataTypeConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDataTypeConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the Data Type configuration from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbDataTypeConfigurationModel>>} - The data type configuration.
	 * @memberof UmbDataTypeConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbDataTypeConfigurationModel>> {
		const { data, error } = await tryExecute(this, DataTypeService.getDataTypeConfiguration());

		if (data) {
			const mappedData: UmbDataTypeConfigurationModel = {
				offerDeprecatedPropertyEditors: data.offerDeprecatedPropertyEditors,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
