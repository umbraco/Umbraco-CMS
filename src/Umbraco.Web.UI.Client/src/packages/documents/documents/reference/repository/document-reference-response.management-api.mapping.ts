import type { UmbDocumentReferenceModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import {
	DocumentVariantStateModel,
	type DocumentReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
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
			id: data.id,
			name: data.name,
			published: data.published,
			// TODO: this is a hardcoded array until the server can return the correct variants array
			variants: [
				{
					culture: null,
					name: data.name ?? '',
					state: data.published ? DocumentVariantStateModel.PUBLISHED : null,
				},
			],
			unique: data.id,
		};
	}
}

export { UmbDocumentReferenceResponseManagementApiDataMapping as api };
