import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE } from '../user-permission.js';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentPropertyValuePermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentPropertyValueUserPermissionFromManagementApiDataMapping
	extends UmbControllerBase
	implements
		UmbDataSourceDataMapping<
			DocumentPropertyValuePermissionPresentationModel,
			UmbDocumentPropertyValueUserPermissionModel
		>
{
	async map(
		data: DocumentPropertyValuePermissionPresentationModel,
	): Promise<UmbDocumentPropertyValueUserPermissionModel> {
		return {
			$type: data.$type,
			userPermissionType: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE,
			documentType: {
				unique: data.documentType.id,
			},
			propertyType: {
				unique: data.propertyType.id,
			},
			verbs: data.verbs,
		};
	}
}

export { UmbDocumentPropertyValueUserPermissionFromManagementApiDataMapping as api };
