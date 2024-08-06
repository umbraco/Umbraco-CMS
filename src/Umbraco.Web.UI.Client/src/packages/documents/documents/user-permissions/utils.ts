import type {
	DocumentPermissionPresentationModel,
	UnknownTypePermissionPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param permission
 */
export function isDocumentUserPermission(
	permission: DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel,
): permission is DocumentPermissionPresentationModel {
	return (permission as DocumentPermissionPresentationModel).$type === 'DocumentPermissionPresentationModel';
}
