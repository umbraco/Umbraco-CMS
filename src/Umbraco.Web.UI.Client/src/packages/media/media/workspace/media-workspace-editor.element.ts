import type { UmbMediaVariantOptionModel } from '../types.js';
import { UmbMediaWorkspaceSplitViewElement } from './media-workspace-split-view.element.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from './media-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-media-workspace-editor')
export class UmbMediaWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbMediaWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;
	#variants?: Array<UmbMediaVariantOptionModel>;
	#isForbidden = false;

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _loading?: boolean = true;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
			this.#observeForbidden();
			this.#observeLoading();
		});
	}

	#observeVariants() {
		this.observe(
			this.#workspaceContext?.variantOptions,
			(options) => {
				this.#variants = options;
				this._generateRoutes();
			},
			'_observeVariants',
		);
	}

	#observeForbidden() {
		this.observe(
			this.#workspaceContext?.forbidden.isOn,
			(forbidden) => {
				this.#isForbidden = forbidden ?? false;
				this._generateRoutes();
			},
			'_observeForbidden',
		);
	}

	#observeLoading() {
		this.observe(
			this.#workspaceContext?.loading.isOn,
			(loading) => {
				this._loading = loading ?? false;
			},
			'_observeLoading',
		);
	}

	private async _generateRoutes() {
		if (!this.#variants || this.#variants.length === 0) {
			this._routes = [];
			this.#ensureForbiddenRoute(this._routes);
			return;
		}

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		this.#variants?.forEach((variantA) => {
			this.#variants?.forEach((variantB) => {
				routes.push({
					// TODO: When implementing Segments, be aware if using the unique is URL Safe... [NL]
					path: variantA.unique + '_&_' + variantB.unique,
					component: this._splitViewElement,
					setup: (_component, info) => {
						// Set split view/active info..
						this.#workspaceContext?.splitView.setVariantParts(info.match.fragments.consumed);
					},
				});
			});
		});

		// Single view:
		this.#variants?.forEach((variant) => {
			routes.push({
				// TODO: When implementing Segments, be aware if using the unique is URL Safe... [NL]
				path: variant.unique,
				component: this._splitViewElement,
				setup: (_component, info) => {
					// cause we might come from a split-view, we need to reset index 1.
					this.#workspaceContext?.splitView.removeActiveVariant(1);
					this.#workspaceContext?.splitView.handleVariantFolderPart(0, info.match.fragments.consumed);
				},
			});
		});

		if (routes.length !== 0 && this.#variants?.length) {
			// Using first single view as the default route for now (hence the math below):
			routes.push({
				path: '',
				pathMatch: 'full',
				redirectTo: routes[this.#variants.length * this.#variants.length]?.path,
			});
		}

		this.#ensureForbiddenRoute(routes);

		this._routes = routes;
	}

	/**
	 * Ensure that there is a route to handle forbidden access.
	 * This route will display a forbidden message when the user does not have permission to access certain resources.
	 * Also handles not found routes.
	 * @param {Array<UmbRoute>} routes - The array of routes to append the forbidden route to
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
		this.#workspaceContext?.splitView.setWorkspaceRoute(e.target.absoluteRouterPath);
	};

	override render() {
		return !this._loading && this._routes && this._routes.length > 0
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

export default UmbMediaWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-editor': UmbMediaWorkspaceEditorElement;
	}
}
