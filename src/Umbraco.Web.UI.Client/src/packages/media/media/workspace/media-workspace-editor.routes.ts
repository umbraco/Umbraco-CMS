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
 * Media types currently hardcode `ContentVariation.Nothing`, so the variant routes are dead in
 * production today. The dynamic-route scaffolding is kept in sync with the document editor so
 * the n² split-view route table cannot reappear here if media gains variants later.
 * @param {UmbBuildMediaWorkspaceRoutesArgs} args - the inputs needed to construct the routes.
 * @returns {Array<UmbRoute>} the routes to install on the workspace router slot.
 */
export function buildMediaWorkspaceRoutes(args: UmbBuildMediaWorkspaceRoutesArgs): Array<UmbRoute> {
	const notFoundComponent = async () => {
		const router = await import('@umbraco-cms/backoffice/router');
		return args.getIsForbidden() ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
	};

	const catchAllRoute: UmbRoute = {
		path: '**',
		component: notFoundComponent,
	};

	if (args.variants.length === 0 || !args.splitView) {
		return [catchAllRoute];
	}

	const { splitView, variants, splitViewComponent } = args;

	return [
		{
			// TODO: When implementing Segments, be aware if using the unique is URL Safe... [NL]
			path: ':variantPath',
			resolve: async (info) => {
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
			pathMatch: 'full',
			redirectTo: variants[0]?.unique,
		},
		catchAllRoute,
	];
}
