import type { UmbContextualUserPermissionModel } from '../types.js';
import type { UnknownTypePermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A type guard to check if a permission is a contextual user permission.
 * @param { unknown } permission The permission to check.
 * @returns { boolean } True if the permission is a contextual user permission, otherwise false.
 */
export function isContextualUserPermission(permission: unknown): permission is UmbContextualUserPermissionModel {
	return (
		(permission as UnknownTypePermissionPresentationModel).$type === 'UnknownTypePermissionPresentationModel' &&
		(permission as UmbContextualUserPermissionModel).context !== undefined
	);
}
