import type { UmbMediaVariantOptionModel } from '../types.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';

export interface UmbBuildMediaWorkspaceRoutesArgs {
	variants: ReadonlyArray<UmbMediaVariantOptionModel>;
	splitViewComponent: HTMLElement;
	splitView: UmbWorkspaceSplitViewManager | undefined;
	getIsForbidden: () => boolean;
}

/**
 * Build the routes for the media workspace editor.
 *
 * Media does not currently support variants (MediaType.Variations is hardcoded to Nothing), so
 * in practice this only produces the forbidden catch-all today. The dynamic-route scaffolding is
 * kept in sync with the document editor so the n² split-view loop cannot regress here if media
 * gains variants later.
 * @param {UmbBuildMediaWorkspaceRoutesArgs} args - the inputs needed to construct the routes.
 * @returns {Array<UmbRoute>} the routes to install on the workspace router slot.
 */
export function buildMediaWorkspaceRoutes(args: UmbBuildMediaWorkspaceRoutesArgs): Array<UmbRoute> {
	const forbiddenRoute: UmbRoute = {
		path: '**',
		component: async () => {
			const router = await import('@umbraco-cms/backoffice/router');
			return args.getIsForbidden() ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
		},
	};

	if (args.variants.length === 0 || !args.splitView) {
		return [forbiddenRoute];
	}

	const { splitView, variants, splitViewComponent } = args;

	return [
		{
			path: ':variantPath',
			component: splitViewComponent,
			setup: (_component, info) => {
				const consumed = info.match.fragments.consumed;
				if (consumed.includes('_&_')) {
					splitView.setVariantParts(consumed);
				} else {
					splitView.removeActiveVariant(1);
					splitView.handleVariantFolderPart(0, consumed);
				}
			},
		},
		{
			path: '',
			pathMatch: 'full',
			redirectTo: variants[0]?.unique,
		},
		forbiddenRoute,
	];
}
