import type { UmbDocumentSegmentFilterModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbSegmentModel, UmbSegmentResponseModel } from '@umbraco-cms/backoffice/segment';

export class UmbDocumentSegmentRepository extends UmbRepositoryBase {
	/**
	 * Get available segment options for a document by its ID.
	 * @param {string} unique The unique identifier of the document.
	 * @param {UmbDocumentSegmentFilterModel} filter The filter options to apply.
	 * @returns A promise that resolves with the available segment options.
	 */
	async getDocumentByIdSegmentOptions(
		unique: string,
		filter: UmbDocumentSegmentFilterModel,
	): Promise<UmbRepositoryResponse<UmbSegmentResponseModel>> {
		const { data, error } = await tryExecute(
			this,
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			DocumentService.getDocumentByIdAvailableSegmentOptions({ path: { id: unique }, query: filter }),
		);

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbSegmentModel = {
					alias: item.alias,
					name: item.name,
					// eslint-disable-next-line @typescript-eslint/no-deprecated
					cultures: item.cultures,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
