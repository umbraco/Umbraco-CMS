// TODO: Uncomment this when `ElementReferenceResponseModel` API type is available. [LK:2026-01-06]
// import type { UmbElementReferenceModel } from '../types.js';
// import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
// import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
// import type { ElementReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
// import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

// export class UmbElementReferenceResponseManagementApiDataMapping
// 	extends UmbControllerBase
// 	implements UmbDataSourceDataMapping<ElementReferenceResponseModel, UmbElementReferenceModel>
// {
// 	async map(data: ElementReferenceResponseModel): Promise<UmbElementReferenceModel> {
// 		return {
// 			documentType: {
// 				alias: data.ElementType.alias!,
// 				icon: data.ElementType.icon!,
// 				name: data.ElementType.name!,
// 				unique: data.ElementType.id,
// 			},
// 			entityType: UMB_ELEMENT_ENTITY_TYPE,
// 			variants: data.variants.map((variant) => ({
// 				...variant,
// 				// TODO: Review if we should make `culture` to allow `undefined`. [LK]
// 				culture: variant.culture ?? null,
// 			})),
// 			unique: data.id,
// 		};
// 	}
// }

// export { UmbElementReferenceResponseManagementApiDataMapping as api };
