import type { UmbPathPattern, UmbPathPatternParamsType } from './path-pattern.class.js';
import { ensurePathEndsWithSlash, removeInitialSlashFromPath } from '@umbraco-cms/backoffice/utils';

export interface UmbWorkspaceLink {
	/**
	 * The path to the workspace.
	 */
	href: string;
	/**
	 * Set when following `href` leaves whatever the host is currently showing, so the link has to open
	 * in a new browser tab. Left out when the link navigates within the current view.
	 */
	target?: '_blank';
}

export interface UmbGenerateWorkspaceLinkArgs<ParamsType extends UmbPathPatternParamsType> {
	/**
	 * The path pattern of the workspace to link to.
	 */
	pattern: UmbPathPattern<ParamsType>;
	/**
	 * The parameters of the path pattern.
	 */
	params: ParamsType;
	/**
	 * The path of the host's workspace modal route, as handed out by `observeRouteBuilder`. Leave it out
	 * when the host has no ancestor route context.
	 */
	routePath?: string;
}

/**
 * Generate a link to a workspace from a host that may or may not sit in a routable context.
 * @param {UmbGenerateWorkspaceLinkArgs} args - The pattern and parameters of the workspace, and the host's route path.
 * @returns {UmbWorkspaceLink} The path to the workspace, and how to open it.
 * @example
 * ```ts
 * umbGenerateWorkspaceLink({
 *   pattern: UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
 *   params: { unique: this._unique },
 *   routePath: this._editPath,
 * });
 * ```
 */
export function umbGenerateWorkspaceLink<ParamsType extends UmbPathPatternParamsType>(
	args: UmbGenerateWorkspaceLinkArgs<ParamsType>,
): UmbWorkspaceLink {
	const { pattern, params, routePath } = args;

	// Without a route path the host has no ancestor route context, so the workspace cannot be expressed
	// as a route relative to it. It is still reachable by its absolute path, but following that leaves
	// whatever the host is showing, so it opens in a new tab instead.
	if (!routePath) {
		return { href: pattern.generateAbsolute(params), target: '_blank' };
	}

	return {
		href: ensurePathEndsWithSlash(routePath) + removeInitialSlashFromPath(pattern.generateLocal(params)),
	};
}
