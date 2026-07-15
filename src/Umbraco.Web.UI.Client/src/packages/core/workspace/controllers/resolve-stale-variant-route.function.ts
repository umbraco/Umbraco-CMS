import { UMB_WORKSPACE_PATH_VARIANT_DELIMITER } from './constants.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbStaleVariantRouteResolverVariant {
	culture: string | null;
	segment: string | null;
	unique: string;
}

export interface UmbStaleVariantRouteResolverArgs {
	/**
	 * The current location pathname, e.g. window.location.pathname.
	 */
	currentPath: string;
	/**
	 * The absolute router path of the workspace, without a trailing slash.
	 */
	workspaceRoute: string;
	/**
	 * The available variant options.
	 */
	variants: Array<UmbStaleVariantRouteResolverVariant>;
	/**
	 * The culture currently selected in the app language switcher.
	 */
	appCulture?: string;
}

/**
 * Checks whether the variant part of a workspace URL refers to variant options that no longer exist —
 * e.g. after the content type's Vary by Culture setting changed — and resolves a corrected path.
 * Split-view parts are resolved individually and deduplicated.
 * @param {UmbStaleVariantRouteResolverArgs} args - The current path, workspace route, variant options and app culture.
 * @returns {string | null} The corrected path without the query string, or null when the URL is valid or not applicable.
 */
export function resolveStaleVariantRoute(args: UmbStaleVariantRouteResolverArgs): string | null {
	const { currentPath, workspaceRoute, variants, appCulture } = args;
	if (variants.length === 0) return null;

	const routePrefix = workspaceRoute + '/';
	if (!currentPath.startsWith(routePrefix)) return null;

	const remainingPath = currentPath.substring(routePrefix.length);
	if (remainingPath.length === 0) return null;

	// Separate the variant part from any trailing path (e.g. /view/info):
	const slashIndex = remainingPath.indexOf('/');
	const variantPart = slashIndex === -1 ? remainingPath : remainingPath.substring(0, slashIndex);
	const pathSuffix = slashIndex === -1 ? '' : remainingPath.substring(slashIndex);

	// Skip while a modal is layered on top — closing it restores the pre-modal URL and dispatches
	// a changestate event, at which point the correction can run against the settled URL:
	if (pathSuffix.split('/').includes('modal')) return null;

	const parts = variantPart.split(UMB_WORKSPACE_PATH_VARIANT_DELIMITER);
	const isValid = (part: string) => variants.some((v) => v.unique === part);

	if (parts.every(isValid)) return null;

	const resolvedParts = parts.map((part) => {
		if (isValid(part)) return part;
		const variantId = UmbVariantId.FromString(part);
		// Prefer the same culture without a segment, then the app culture, then invariant, then the first option:
		const fallback =
			variants.find((v) => v.culture === variantId.culture && v.segment === null) ??
			variants.find((v) => v.culture === appCulture && v.segment === null) ??
			variants.find((v) => v.culture === null && v.segment === null) ??
			variants[0];
		return fallback.unique;
	});

	const uniqueParts = [...new Set(resolvedParts)];

	return `${workspaceRoute}/${uniqueParts.join(UMB_WORKSPACE_PATH_VARIANT_DELIMITER)}${pathSuffix}`;
}
