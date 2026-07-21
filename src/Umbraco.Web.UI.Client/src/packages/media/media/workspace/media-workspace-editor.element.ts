import type { UmbMediaVariantOptionModel } from '../types.js';
import { UmbMediaWorkspaceSplitViewElement } from './media-workspace-split-view.element.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from './media-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

// TODO: Refactor across all four content workspace editors (document, document blueprint, media, member) to use a base component. [NL]
@customElement('umb-media-workspace-editor')
export class UmbMediaWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbMediaWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;

	#workspaceRoute?: string;
	#variants?: Array<Pick<UmbMediaVariantOptionModel, 'culture' | 'segment' | 'unique'>>;

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _loading?: boolean = true;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
			this.#observeLoading();
		});
	}

	#observeVariants() {
		this.observe(
			this.#workspaceContext
				? createObservablePart(this.#workspaceContext.variantOptions, (variants) =>
						variants.map((v) => ({
							culture: v.culture,
							segment: v.segment,
							unique: v.unique,
						})),
					)
				: undefined,
			(options) => {
				this.#variants = options;
				this._generateRoutes();
			},
			'_observeVariants',
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
			this._routes = [this.#createNotFoundRoute()];
			return;
		}

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		routes.push({
			path: '/:variantPath/',
			preserveQuery: true,
			component: this._splitViewElement,
			setup: async (_component, info) => {
				const variants = this.#variants;
				if (!variants) {
					throw new Error('Variants are not available when resolving the route.');
				}
				if (!this.#workspaceContext) {
					throw new Error('Workspace context is not available when resolving the route.');
				}

				const consumed = info.match.fragments.consumed;

				this.#workspaceContext?.splitView.setVariantParts(consumed);
			},
		});

		if (routes.length !== 0) {
			// Find a decent variant to use as the default route:
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
					const view = openCollection ? `/view/collection` : '';

					// Is there a path matching the current culture?
					let variant = this.#variants?.find((v) => !v.culture);

					if (!variant) {
						// If none then just use the first variant as a fallback.
						variant = this.#variants?.[0];
					}

					history.replaceState({}, '', `${this.#workspaceRoute}/${variant?.unique}${view}`);
				},
			});
		}

		routes.push(this.#createNotFoundRoute());

		this._routes = routes;
	}

	#createNotFoundRoute(): UmbRoute {
		return {
			path: '**',
			component: async () => {
				const router = await import('@umbraco-cms/backoffice/router');
				return this.#workspaceContext?.forbidden.getIsOn()
					? router.UmbRouteForbiddenElement
					: router.UmbRouteNotFoundElement;
			},
		};
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceRoute = e.target.absoluteRouterPath;
		this.#workspaceContext?.splitView.setWorkspaceRoute(this.#workspaceRoute);
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
