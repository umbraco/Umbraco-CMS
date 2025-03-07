import type { UmbUiUserPermissionModel } from '../types.js';
import type { UnknownTypePermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A type guard to check if a permission is a UI user permission.
 * @param { unknown } permission The permission to check.
 * @returns { boolean } True if the permission is a UI user permission, otherwise false.
 */
export function isUiUserPermission(permission: unknown): permission is UmbUiUserPermissionModel {
	return (
		(permission as UnknownTypePermissionPresentationModel).$type === 'UnknownTypePermissionPresentationModel' &&
		(permission as UmbUiUserPermissionModel).context !== undefined
	);
}
