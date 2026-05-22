import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';

export interface UmbBuildDocumentWorkspaceRoutesArgs {
	variants: ReadonlyArray<UmbDocumentVariantOptionModel>;
	appCulture: string | undefined;
	splitViewComponent: HTMLElement;
	splitView: UmbWorkspaceSplitViewManager | undefined;
	getWorkspaceRoute: () => string | undefined;
	getIsForbidden: () => boolean;
}

/**
 * Build the routes for the document workspace editor.
 *
 * One dynamic route handles every single-variant and split-view path. The split-view manager
 * already parses '_&_' itself, so the route table stays at three entries regardless of how many
 * cultures/segments the document has.
 * @param {UmbBuildDocumentWorkspaceRoutesArgs} args - the inputs needed to construct the routes.
 * @returns {Array<UmbRoute>} the routes to install on the workspace router slot.
 */
export function buildDocumentWorkspaceRoutes(args: UmbBuildDocumentWorkspaceRoutesArgs): Array<UmbRoute> {
	const forbiddenRoute: UmbRoute = {
		path: '**',
		component: async () => {
			const router = await import('@umbraco-cms/backoffice/router');
			return args.getIsForbidden() ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
		},
	};

	if (args.variants.length === 0 || !args.appCulture || !args.splitView) {
		return [forbiddenRoute];
	}

	const { splitView, variants, appCulture, splitViewComponent } = args;

	return [
		{
			path: ':variantPath',
			preserveQuery: true,
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
			preserveQuery: true,
			pathMatch: 'full',
			resolve: async () => {
				const workspaceRoute = args.getWorkspaceRoute();
				if (!workspaceRoute) {
					throw new Error('Workspace route is not available when resolving the default route.');
				}

				const urlSearchParams = new URLSearchParams(window.location.search);
				const openCollection = urlSearchParams.has('openCollection');

				// Prefer the variant whose unique matches the app culture (the default segment for that
				// culture); otherwise fall back to the first variant.
				const target = variants.find((v) => v.unique === appCulture)?.unique ?? variants[0]?.unique;

				if (!target) return;

				history.replaceState({}, '', `${workspaceRoute}/${target}${openCollection ? '/view/collection' : ''}`);
			},
		},
		forbiddenRoute,
	];
}
