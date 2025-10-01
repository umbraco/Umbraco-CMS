import type {
	UmbDocumentSegmentFilterModel,
	UmbDocumentSegmentModel,
	UmbDocumentSegmentResponseModel,
} from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentSegmentRepository extends UmbRepositoryBase {
	/**
	 * Get available segment options for a document by its ID.
	 * @param unique The unique identifier of the document.
	 * @param filter The filter options to apply.
	 * @returns A promise that resolves with the available segment options.
	 */
	async getDocumentByIdSegmentOptions(
		unique: string,
		filter: UmbDocumentSegmentFilterModel,
	): Promise<UmbRepositoryResponse<UmbDocumentSegmentResponseModel>> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentByIdAvailableSegmentOptions({ path: { id: unique }, query: filter }),
		);

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbDocumentSegmentModel = {
					alias: item.alias,
					name: item.name,
					cultures: item.cultures,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
