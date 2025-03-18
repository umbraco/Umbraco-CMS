import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentTypePropertyPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentPropertyValueUserPermissionToManagementApiDataMapping
	extends UmbControllerBase
	implements
		UmbDataSourceDataMapping<
			UmbDocumentPropertyValueUserPermissionModel,
			DocumentTypePropertyPermissionPresentationModel
		>
{
	async map(
		data: UmbDocumentPropertyValueUserPermissionModel,
	): Promise<DocumentTypePropertyPermissionPresentationModel> {
		return {
			$type: 'DocumentTypePropertyPermissionPresentationModel',
			documentType: {
				id: data.documentType.unique,
			},
			propertyType: {
				id: data.propertyType.unique,
			},
			verbs: data.verbs,
		};
	}
}

export { UmbDocumentPropertyValueUserPermissionToManagementApiDataMapping as api };
