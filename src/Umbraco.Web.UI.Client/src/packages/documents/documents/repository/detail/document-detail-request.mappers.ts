import type { UmbDocumentDetailModel } from '../../types.js';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Maps a Document detail model to the create request body.
 * Shared by the detail create endpoint and the publishing create-and-publish endpoint.
 * @param {UmbDocumentDetailModel} model - The Document to create
 * @param {string | null} parentUnique - The unique of the parent to create under
 * @returns {CreateDocumentRequestModel} The create request body
 */
export function umbMapDocumentCreateRequestBody(
	model: UmbDocumentDetailModel,
	parentUnique: string | null,
): CreateDocumentRequestModel {
	return {
		id: model.unique,
		parent: parentUnique ? { id: parentUnique } : null,
		documentType: { id: model.documentType.unique },
		template: model.template ? { id: model.template.unique } : null,
		values: model.values,
		variants: model.variants,
	};
}

/**
 * Maps a Document detail model to the update request body.
 * Shared by the detail update endpoint and the publishing update-and-publish endpoint.
 * @param {UmbDocumentDetailModel} model - The Document to update
 * @returns {UpdateDocumentRequestModel} The update request body
 */
export function umbMapDocumentUpdateRequestBody(model: UmbDocumentDetailModel): UpdateDocumentRequestModel {
	return {
		template: model.template ? { id: model.template.unique } : null,
		values: model.values,
		variants: model.variants,
	};
}
