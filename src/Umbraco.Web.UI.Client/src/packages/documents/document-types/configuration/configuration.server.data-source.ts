import type { UmbDocumentTypeConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentTypeConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the document type configuration from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbDocumentTypeConfigurationModel>>} - The document type configuration.
	 * @memberof UmbDocumentTypeConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbDocumentTypeConfigurationModel>> {
		const { data, error } = await tryExecute(this, DocumentTypeService.getDocumentTypeConfiguration());

		if (data) {
			const mappedData: UmbDocumentTypeConfigurationModel = {
				dataTypesCanBeChanged: data.dataTypesCanBeChanged === 'True' ? true : false,
				disableTemplates: data.disableTemplates,
				useSegments: data.useSegments,
				reservedFieldNames: data.reservedFieldNames,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
