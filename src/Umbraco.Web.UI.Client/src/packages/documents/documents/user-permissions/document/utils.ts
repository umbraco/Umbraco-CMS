import type { DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param permission
 */
export function isDocumentUserPermission(permission: unknown): permission is DocumentPermissionPresentationModel {
	return (permission as DocumentPermissionPresentationModel).$type === 'DocumentPermissionPresentationModel';
}
