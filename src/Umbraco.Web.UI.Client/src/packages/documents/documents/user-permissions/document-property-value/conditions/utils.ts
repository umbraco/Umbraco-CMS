import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE } from '../user-permission.js';

/**
 * Checks if the given permission is a document property value user permission.
 * @param {unknown} permission - The permission to check
 * @returns {boolean} True if the permission is a permission for document property values
 */
export function isDocumentPropertyValueUserPermission(
	permission: unknown,
): permission is UmbDocumentPropertyValueUserPermissionModel {
	return (
		(permission as UmbDocumentPropertyValueUserPermissionModel).userPermissionType ===
		UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE
	);
}
