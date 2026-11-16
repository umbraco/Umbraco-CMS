import type { IPermissionPresentationModelElementPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

interface UmbPermissionVerbsConfig {
	allOf?: Array<string>;
	oneOf?: Array<string>;
}

export interface UmbResolveElementPermissionVerbsArgs {
	/** The user's permissions. Entries that are not element permissions are ignored. */
	permissions: Array<unknown> | undefined;
	/** The verbs that apply where no granular element permission is defined. */
	fallbackPermissions: Array<string> | undefined;
	/** The unique of the element or folder, preceded by its ancestors. */
	path: Array<string>;
}

const isElementPermission = (
	permission: unknown,
): permission is IPermissionPresentationModelElementPermissionPresentationModel =>
	(permission as IPermissionPresentationModelElementPermissionPresentationModel)?.$type ===
	'ElementPermissionPresentationModel';

/**
 * Resolves the element permission verbs that apply at a location in the element tree.
 * The closest granular element permission on the path wins, otherwise the fallback permissions apply.
 * @param {UmbResolveElementPermissionVerbsArgs} args - The permissions and the path to resolve for.
 * @returns {Array<string>} The verbs that apply at the location.
 */
export function umbResolveElementPermissionVerbs(args: UmbResolveElementPermissionVerbsArgs): Array<string> {
	const fallbackVerbs = args.fallbackPermissions ?? [];
	const elementPermissions = (args.permissions ?? []).filter(isElementPermission);
	const permissionsById = new Map(elementPermissions.map((permission) => [permission.element.id, permission]));

	const closestId = [...args.path].reverse().find((unique) => permissionsById.has(unique));

	return closestId ? permissionsById.get(closestId)!.verbs : fallbackVerbs;
}

/**
 * Checks a set of verbs against the `allOf` and `oneOf` requirements.
 * Not permitted when no requirements are given.
 * @param {Array<string>} verbs - The verbs the user has.
 * @param {UmbPermissionVerbsConfig} config - The required verbs.
 * @returns {boolean} Whether the verbs satisfy the requirements.
 */
export function umbElementPermissionVerbsSatisfy(verbs: Array<string>, config: UmbPermissionVerbsConfig): boolean {
	if (!config.allOf?.length && !config.oneOf?.length) return false;

	const allOfPermitted = config.allOf?.length ? config.allOf.every((verb) => verbs.includes(verb)) : true;
	const oneOfPermitted = config.oneOf?.length ? config.oneOf.some((verb) => verbs.includes(verb)) : true;

	return allOfPermitted && oneOfPermitted;
}
