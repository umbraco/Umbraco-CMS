import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementDetailModel } from '../../types.js';
import type { ElementResponseModel, PublishedElementResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Maps an Element response model to the Element detail model.
 * Shared by the detail read endpoint and the publishing published endpoint.
 * @param {ElementResponseModel | PublishedElementResponseModel} data - The Element response from the server
 * @returns {UmbElementDetailModel} The Element detail model
 */
export function umbMapElementResponseToDetailModel(
	data: ElementResponseModel | PublishedElementResponseModel,
): UmbElementDetailModel {
	return {
		entityType: UMB_ELEMENT_ENTITY_TYPE,
		unique: data.id,
		values: data.values.map((value) => {
			return {
				editorAlias: value.editorAlias,
				culture: value.culture || null,
				segment: value.segment || null,
				alias: value.alias,
				value: value.value,
			};
		}),
		variants: data.variants.map((variant) => {
			return {
				culture: variant.culture || null,
				segment: variant.segment || null,
				state: variant.state,
				name: variant.name,
				publishDate: variant.publishDate || null,
				createDate: variant.createDate,
				updateDate: variant.updateDate,
				scheduledPublishDate: variant.scheduledPublishDate || null,
				scheduledUnpublishDate: variant.scheduledUnpublishDate || null,
				flags: variant.flags,
			};
		}),
		documentType: {
			unique: data.documentType.id,
			collection: null,
			icon: data.documentType.icon,
		},
		isTrashed: data.isTrashed,
		flags: data.flags,
	};
}
