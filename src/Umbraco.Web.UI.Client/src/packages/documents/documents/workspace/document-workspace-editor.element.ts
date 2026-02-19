import type { UmbDocumentVariantOptionModel } from '../types.js';
import { UmbDocumentWorkspaceSplitViewElement } from './document-workspace-split-view.element.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';

// TODO: This seem fully identical with Media Workspace Editor, so we can refactor this to a generic component. [NL]
@customElement('umb-document-workspace-editor')
export class UmbDocumentWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbDocumentWorkspaceSplitViewElement();

	#appLanguage?: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	#workspaceRoute?: string;
	#appCulture?: string;
	#variants?: Array<UmbDocumentVariantOptionModel>;
	#isForbidden = false;

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _loading?: boolean = true;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguage = instance;
			this.observe(this.#appLanguage?.appLanguageCulture, (appCulture) => {
				this.#appCulture = appCulture;
				this.#generateRoutes();
			});
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(
				this.#workspaceContext?.variantOptions,
				(variants) => {
					this.#variants = variants;
					this.#generateRoutes();
				},
				'_observeVariants',
			);

			this.observe(
				this.#workspaceContext?.forbidden.isOn,
				(isForbidden) => {
					this.#isForbidden = isForbidden ?? false;
					this.#generateRoutes();
				},
				'_observeForbidden',
			);

			this.observe(
				this.#workspaceContext?.loading.isOn,
				(loading) => {
					this._loading = loading ?? false;
				},
				'_observeLoading',
			);
		});
	}

	#generateRoutes() {
		if (!this.#variants || this.#variants.length === 0 || !this.#appCulture) {
			this._routes = [];
			this.#ensureForbiddenRoute(this._routes);
			return;
		}

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		this.#variants.forEach((variantA) => {
			this.#variants!.forEach((variantB) => {
				routes.push({
					// TODO: When implementing Segments, be aware if using the unique still is URL Safe, cause its most likely not... [NL]
					path: variantA.unique + '_&_' + variantB.unique,
					preserveQuery: true,
					component: this._splitViewElement,
					setup: (_component, info) => {
						// Set split view/active info..
						this.#workspaceContext?.splitView.setVariantParts(info.match.fragments.consumed);
					},
				});
			});
		});

		// Single view:
		this.#variants.forEach((variant) => {
			routes.push({
				// TODO: When implementing Segments, be aware if using the unique still is URL Safe, cause its most likely not... [NL]
				path: variant.unique,
				preserveQuery: true,
				component: this._splitViewElement,
				setup: (_component, info) => {
					// cause we might come from a split-view, we need to reset index 1.
					this.#workspaceContext?.splitView.removeActiveVariant(1);
					this.#workspaceContext?.splitView.handleVariantFolderPart(0, info.match.fragments.consumed);
				},
			});
		});

		if (routes.length !== 0) {
			// Using first single view as the default route for now (hence the math below):
			routes.push({
				path: '',
				preserveQuery: true,
				pathMatch: 'full',
				resolve: async () => {
					if (!this.#workspaceContext) {
						throw new Error('Workspace context is not available when resolving the default route.');
					}

					// get current get variables from url, and check if openCollection is set:
					const urlSearchParams = new URLSearchParams(window.location.search);
					const openCollection = urlSearchParams.has('openCollection');

					// Is there a path matching the current culture?
					let path = routes.find((route) => route.path === this.#appCulture)?.path;

					if (!path) {
						// if not is there then a path matching the first variant unique.
						path = routes.find((route) => route.path === this.#variants?.[0]?.unique)?.path;
					}

					if (!path) {
						// If not is there then a path matching the first variant unique that is not a culture.
						// TODO: Notice: here is a specific index used for fallback, this could be made more solid [NL]
						path = routes[routes.length - 3].path;
					}

					history.replaceState({}, '', `${this.#workspaceRoute}/${path}${openCollection ? `/view/collection` : ''}`);
				},
			});
		}

		this.#ensureForbiddenRoute(routes);

		this._routes = routes;
	}

	/**
	 * Ensure that there is a route to handle forbidden access.
	 * This route will display a forbidden message when the user does not have permission to access certain resources.
	 * Also handles not found routes.
	 * @param routes
	 */
	#ensureForbiddenRoute(routes: Array<UmbRoute> = []) {
		routes.push({
			path: '**',
			component: async () => {
				const router = await import('@umbraco-cms/backoffice/router');
				return this.#isForbidden ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
			},
		});
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceRoute = e.target.absoluteRouterPath;
		this.#workspaceContext?.splitView.setWorkspaceRoute(this.#workspaceRoute);
	};

	override render() {
		return !this._loading && this._routes
			? html`<umb-router-slot .routes=${this._routes} @init=${this._gotWorkspaceRoute}></umb-router-slot>`
			: html`<umb-view-loader></umb-view-loader>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbDocumentWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-editor': UmbDocumentWorkspaceEditorElement;
	}
}
