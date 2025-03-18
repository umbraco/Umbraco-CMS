import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';

/**
 *
 * @param permission
 * @returns {boolean} True if the permission is a document-property-value' permission
 */
export function isDocumentPropertyValueUserPermission(
	permission: unknown,
): permission is UmbDocumentPropertyValueUserPermissionModel {
	return (permission as UmbDocumentPropertyValueUserPermissionModel).permissionType === 'document-property-value';
}
