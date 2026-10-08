import type { UmbElementVariantOptionModel } from '../types.js';
import { UmbElementWorkspaceSplitViewElement } from './element-workspace-split-view.element.js';
import { UMB_ELEMENT_WORKSPACE_CONTEXT } from './element-workspace.context-token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-element-workspace-editor')
export class UmbElementWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbElementWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_ELEMENT_WORKSPACE_CONTEXT.TYPE;
	#variants?: Array<UmbElementVariantOptionModel>;
	#isForbidden = false;

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _loading?: boolean = true;

	constructor() {
		super();
		this.consumeContext(UMB_ELEMENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
			this.#observeForbidden();
			this.#observeLoading();
		});
	}

	#observeVariants() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.variantOptions,
			(variants) => {
				this.#variants = variants;
				this._generateRoutes();
			},
			'_observeVariants',
		);
	}

	#observeForbidden() {
		this.observe(
			this.#workspaceContext?.forbidden.isOn,
			(isForbidden) => {
				this.#isForbidden = isForbidden ?? false;
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

	private _generateRoutes() {
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

		routes.push({
			path: `**`,
			component: async () => {
				const router = await import('@umbraco-cms/backoffice/router');
				return this.#isForbidden ? router.UmbRouteForbiddenElement : router.UmbRouteNotFoundElement;
			},
		});

		this._routes = routes;
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceContext?.splitView.setWorkspaceRoute(e.target.absoluteRouterPath);
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

				--uui-color-invalid: var(--uui-color-warning);
				--uui-color-invalid-emphasis: var(--uui-color-warning-emphasis);
				--uui-color-invalid-standalone: var(--uui-color-warning-standalone);
				--uui-color-invalid-contrast: var(--uui-color-warning-contrast);
			}
		`,
	];
}

export default UmbElementWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-editor': UmbElementWorkspaceEditorElement;
	}
}
