import type { UmbDocumentReferenceModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { DocumentReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataMapper } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentReferenceResponseModelMapper
	extends UmbControllerBase
	implements UmbDataMapper<DocumentReferenceResponseModel, UmbDocumentReferenceModel>
{
	async map(data: DocumentReferenceResponseModel): Promise<UmbDocumentReferenceModel> {
		return {
			documentType: {
				alias: data.documentType.alias,
				icon: data.documentType.icon,
				name: data.documentType.name,
			},
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			id: data.id,
			name: data.name,
			published: data.published,
			unique: data.id,
		};
	}
}

export { UmbDocumentReferenceResponseModelMapper as api };
