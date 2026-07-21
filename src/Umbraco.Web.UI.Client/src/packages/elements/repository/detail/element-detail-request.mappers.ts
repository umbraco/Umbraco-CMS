import type { UmbElementDetailModel } from '../../types.js';
import type {
	CreateElementRequestModel,
	UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Maps an Element detail model to the create request body.
 * Shared by the detail create endpoint and the publishing create-and-publish endpoint.
 * @param {UmbElementDetailModel} model - The Element to create
 * @param {string | null} parentUnique - The unique of the parent to create under
 * @returns {CreateElementRequestModel} The create request body
 */
export function umbMapElementCreateRequestBody(
	model: UmbElementDetailModel,
	parentUnique: string | null,
): CreateElementRequestModel {
	return {
		id: model.unique,
		parent: parentUnique ? { id: parentUnique } : null,
		documentType: { id: model.documentType.unique },
		values: model.values,
		variants: model.variants,
	};
}

/**
 * Maps an Element detail model to the update request body.
 * Shared by the detail update endpoint and the publishing update-and-publish endpoint.
 * @param {UmbElementDetailModel} model - The Element to update
 * @returns {UpdateElementRequestModel} The update request body
 */
export function umbMapElementUpdateRequestBody(model: UmbElementDetailModel): UpdateElementRequestModel {
	return {
		values: model.values,
		variants: model.variants,
	};
}
