import type { UmbDocumentConfigurationModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentConfigurationServerDataSource extends UmbControllerBase {
	/**
	 * Gets the document configuration from the server.
	 * @returns {Promise<UmbDataSourceResponse<UmbDocumentConfigurationModel>>} - The document configuration.
	 * @memberof UmbDocumentConfigurationServerDataSource
	 */
	async getConfiguration(): Promise<UmbDataSourceResponse<UmbDocumentConfigurationModel>> {
		const { data, error } = await tryExecute(this, DocumentService.getDocumentConfiguration());

		if (data) {
			const mappedData: UmbDocumentConfigurationModel = {
				disableDeleteWhenReferenced: data.disableDeleteWhenReferenced,
				disableUnpublishWhenReferenced: data.disableUnpublishWhenReferenced,
				allowEditInvariantFromNonDefault: data.allowEditInvariantFromNonDefault,
				// eslint-disable-next-line @typescript-eslint/no-deprecated
				allowNonExistingSegmentsCreation: data.allowNonExistingSegmentsCreation,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
