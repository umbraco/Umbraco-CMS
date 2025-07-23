import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentPropertyValuePermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentPropertyValueUserPermissionToManagementApiDataMapping
	extends UmbControllerBase
	implements
		UmbDataSourceDataMapping<
			UmbDocumentPropertyValueUserPermissionModel,
			DocumentPropertyValuePermissionPresentationModel
		>
{
	async map(
		data: UmbDocumentPropertyValueUserPermissionModel,
	): Promise<DocumentPropertyValuePermissionPresentationModel> {
		return {
			$type: 'DocumentPropertyValuePermissionPresentationModel',
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
