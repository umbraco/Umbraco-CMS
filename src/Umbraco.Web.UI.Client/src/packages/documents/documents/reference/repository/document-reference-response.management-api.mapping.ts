import type { UmbDocumentReferenceModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { IReferenceResponseModelDocumentReferenceResponseModel as DocumentReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements UmbDataSourceDataMapping<DocumentReferenceResponseModel, UmbDocumentReferenceModel>
{
	async map(data: DocumentReferenceResponseModel): Promise<UmbDocumentReferenceModel> {
		return {
			documentType: {
				alias: data.documentType.alias!,
				icon: data.documentType.icon!,
				name: data.documentType.name!,
				unique: data.documentType.id,
			},
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			variants: data.variants.map((variant) => ({
				...variant,
				// TODO: Review if we should make `culture` to allow `undefined`. [LK]
				culture: variant.culture ?? null,
			})),
			unique: data.id,
		};
	}
}

export { UmbDocumentReferenceResponseManagementApiDataMapping as api };
