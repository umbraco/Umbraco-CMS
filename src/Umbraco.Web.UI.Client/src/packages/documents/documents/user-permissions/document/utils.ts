import type { IPermissionPresentationModelDocumentPermissionPresentationModel as DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Checks if the given permission is a document user permission.
 * @param {unknown} permission - The permission to check
 * @returns {boolean} True if the permission is a document user permission
 */
export function isDocumentUserPermission(permission: unknown): permission is DocumentPermissionPresentationModel {
	return (permission as DocumentPermissionPresentationModel).$type === 'DocumentPermissionPresentationModel';
}
