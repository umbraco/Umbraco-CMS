import type { UmbElementReferenceModel } from '../types.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { ElementReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbElementReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements UmbDataSourceDataMapping<ElementReferenceResponseModel, UmbElementReferenceModel>
{
	async map(data: ElementReferenceResponseModel): Promise<UmbElementReferenceModel> {
		return {
			documentType: {
				alias: data.documentType.alias!,
				icon: data.documentType.icon!,
				name: data.documentType.name!,
				unique: data.documentType.id,
			},
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			variants: data.variants.map((variant) => ({
				...variant,
				// TODO: Review if we should make `culture` to allow `undefined`. [LK]
				culture: variant.culture ?? null,
				flags: [],
			})),
			unique: data.id,
		};
	}
}

export { UmbElementReferenceResponseManagementApiDataMapping as api };
