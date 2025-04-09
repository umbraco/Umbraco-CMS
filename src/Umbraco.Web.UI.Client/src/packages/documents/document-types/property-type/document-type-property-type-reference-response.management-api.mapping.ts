import type { UmbDocumentTypePropertyTypeReferenceModel } from './types.js';
import { UMB_DOCUMENT_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import type { DocumentTypePropertyTypeReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentTypePropertyTypeReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements
		UmbDataSourceDataMapping<DocumentTypePropertyTypeReferenceResponseModel, UmbDocumentTypePropertyTypeReferenceModel>
{
	async map(data: DocumentTypePropertyTypeReferenceResponseModel): Promise<UmbDocumentTypePropertyTypeReferenceModel> {
		return {
			alias: data.alias!,
			documentType: {
				alias: data.documentType.alias!,
				icon: data.documentType.icon!,
				name: data.documentType.name!,
				unique: data.documentType.id,
			},
			entityType: UMB_DOCUMENT_TYPE_PROPERTY_TYPE_ENTITY_TYPE,
			name: data.name!,
			unique: data.id,
		};
	}
}

export { UmbDocumentTypePropertyTypeReferenceResponseManagementApiDataMapping as api };
