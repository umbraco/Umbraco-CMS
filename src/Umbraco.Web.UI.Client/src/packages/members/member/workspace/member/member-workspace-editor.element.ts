import type { UmbMemberVariantOptionModel } from '../../types.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from './member-workspace.context-token.js';
import { UmbMemberWorkspaceSplitViewElement } from './member-workspace-split-view.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

@customElement('umb-member-workspace-editor')
export class UmbMemberWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbMemberWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	#workspaceRoute?: string;

	@state()
	private _isForbidden = false;

	@state()
	private _routes?: Array<UmbRoute>;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
			this.#observeForbidden();
		});
	}

	#observeVariants() {
		// TODO: the variantOptions observable is like too broad as this will be triggered then there is any change in the variant options, we need to only update routes when there is a relevant change to them. [NL]
		this.observe(
			this.#workspaceContext?.variantOptions,
			(variants) => this.#generateRoutes(variants ?? []),
			'_observeVariants',
		);
	}

	#observeForbidden() {
		this.observe(
			this.#workspaceContext?.forbidden.isOn,
			(isForbidden) => (this._isForbidden = isForbidden ?? false),
			'_observeForbidden',
		);
	}

	#generateRoutes(variants: Array<UmbMemberVariantOptionModel>) {
		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		variants.forEach((variantA) => {
			variants.forEach((variantB) => {
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
		variants.forEach((variant) => {
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

		if (routes.length !== 0) {
			routes.push({
				path: '',
				pathMatch: 'full',
				//redirectTo: routes[variants.length * variants.length]?.path,
				resolve: async () => {
					if (!this.#workspaceContext) {
						throw new Error('Workspace context is not available when resolving the default route.');
					}

					// Using first single view as the default route for now (hence the math below):
					const path = routes[variants.length * variants.length]?.path;
					history.replaceState({}, '', `${this.#workspaceRoute}/${path}`);
				},
			});
		}

		routes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});

		this._routes = routes;
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceRoute = e.target.absoluteRouterPath;
		this.#workspaceContext?.splitView.setWorkspaceRoute(this.#workspaceRoute);
	};

	override render() {
		if (this._isForbidden) {
			return html`<umb-route-forbidden></umb-route-forbidden>`;
		}
		return this._routes
			? html`<umb-router-slot .routes=${this._routes} @init=${this._gotWorkspaceRoute}></umb-router-slot>`
			: '';
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

export default UmbMemberWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-editor': UmbMemberWorkspaceEditorElement;
	}
}
