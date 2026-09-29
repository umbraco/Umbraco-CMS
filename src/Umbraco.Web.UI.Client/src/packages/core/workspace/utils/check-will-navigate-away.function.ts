import type { UmbWorkspaceRouteManager } from '../controllers/workspace-route-manager.controller.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { umbUrlPatternToString } from '@umbraco-cms/backoffice/utils';

/**
 * Determines whether a pending navigation leaves the workspace that owns the given routes.
 * @param {UmbWorkspaceRouteManager} routeManager The route manager of the workspace.
 * @param {UmbEntityUnique | undefined} unique The unique of the entity currently being edited.
 * @param {string | URL} newUrl The url the navigation is heading towards.
 * @returns {boolean} true if the navigation leaves the workspace.
 */
export function umbWorkspaceWillNavigateAway(
	routeManager: UmbWorkspaceRouteManager,
	unique: UmbEntityUnique | undefined,
	newUrl: string | URL,
): boolean {
	const basePath = routeManager.getAbsolutePath();
	// Before the workspace has been routed we cannot position the match, so we never block.
	if (basePath === undefined) return false;

	const newPath = (newUrl instanceof URL ? newUrl : new URL(newUrl, window.location.origin)).pathname;

	// Check against the active local path
	const sameEntityPaths = [routeManager.getActiveLocalPath()];

	// Check against all routes of the workspace that carry the same unique.
	if (unique) {
		for (const route of routeManager.getRoutes()) {
			// Routes that don't carry the unique keep their unresolved `:param` tokens, so they
			// can never match a concrete URL and are effectively skipped.
			sameEntityPaths.push(umbUrlPatternToString(route.path, { unique }));
		}
	}

	// Run the checks:
	return !sameEntityPaths.some((localPath) => {
		if (localPath === '') return false;
		const absolutePath = `${basePath}/${localPath}`;
		return newPath === absolutePath || newPath.startsWith(`${absolutePath}/`);
	});
}
