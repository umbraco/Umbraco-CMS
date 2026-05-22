import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import type { UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';

export interface UmbBuildDocumentWorkspaceRoutesArgs {
	getVariants: () => ReadonlyArray<UmbDocumentVariantOptionModel>;
	getAppCulture: () => string | undefined;
	splitViewComponent: HTMLElement;
	splitView: UmbWorkspaceSplitViewManager | undefined;
	getWorkspaceRoute: () => string | undefined;
	getIsForbidden: () => boolean;
}

/**
 * Build the routes for the document workspace editor.
 *
 * One dynamic resolver route covers every single-variant and split-view path. The resolver mounts
 * the split-view component for known variants and the NotFound element for unknown ones, without
 * altering the URL. Routes stay a fixed handful of entries regardless of how many variants the
 * document has.
 *
 * Inputs that can change at runtime (variants, appCulture) are passed as getters because
 * umb-router-slot's routes setter skips re-applying routes when only callbacks change — the
 * resolvers must read fresh values at call time rather than relying on captured closures.
 * @param {UmbBuildDocumentWorkspaceRoutesArgs} args - the inputs needed to construct the routes.
 * @returns {Array<UmbRoute>} the routes to install on the workspace router slot.
 */
export function buildDocumentWorkspaceRoutes(args: UmbBuildDocumentWorkspaceRoutesArgs): Array<UmbRoute> {
	const notFoundComponent = async () => {
		const router = await import('@umbraco-cms/backoffice/router');
		return args.getIsForbidden() ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
	};

	const catchAllRoute: UmbRoute = {
		path: '**',
		component: notFoundComponent,
	};

	if (args.getVariants().length === 0 || !args.getAppCulture() || !args.splitView) {
		return [catchAllRoute];
	}

	const { splitView, splitViewComponent } = args;

	return [
		{
			// TODO: When implementing Segments, be aware if using the unique still is URL Safe, cause its most likely not... [NL]
			path: ':variantPath',
			preserveQuery: true,
			resolve: async (info) => {
				const variants = args.getVariants();
				const consumed = info.match.fragments.consumed;

				const parts = consumed.split('_&_');
				const allKnown = parts.every((part) => variants.some((v) => v.unique === part));

				if (!allKnown) {
					// Unknown variant unique in the URL. Render NotFound (or Forbidden) in place — the
					// URL is left untouched so stale bookmarks stay visible to the user.
					const router = await import('@umbraco-cms/backoffice/router');
					const NotFoundCtor = args.getIsForbidden() ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
					if (!(info.slot.firstChild instanceof NotFoundCtor)) {
						info.slot.replaceChildren(new NotFoundCtor());
					}
					return;
				}

				// Mount (or reuse) the split-view component, then push the active variants into the manager.
				if (info.slot.firstChild !== splitViewComponent) {
					info.slot.replaceChildren(splitViewComponent);
				}
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
			resolve: async (info) => {
				// Workspace route may not yet be populated on the wrapper element on initial load (the
				// init event fires after the first navigation); info.slot knows its base path at this
				// point. Return silently rather than throwing — the next navigation will re-resolve
				// once state is fully wired up.
				const workspaceRoute = info.slot.constructAbsolutePath('') || args.getWorkspaceRoute();
				if (!workspaceRoute) return;

				const variants = args.getVariants();
				if (variants.length === 0) return;

				const appCulture = args.getAppCulture();
				const target = variants.find((v) => v.unique === appCulture)?.unique ?? variants[0].unique;

				const urlSearchParams = new URLSearchParams(window.location.search);
				const openCollection = urlSearchParams.has('openCollection');

				history.replaceState({}, '', `${workspaceRoute}/${target}${openCollection ? '/view/collection' : ''}`);
			},
		},
		catchAllRoute,
	];
}
