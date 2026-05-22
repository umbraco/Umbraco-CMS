import type { UmbMediaVariantOptionModel } from '../types.js';
import { UmbMediaWorkspaceSplitViewElement } from './media-workspace-split-view.element.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from './media-workspace.context-token.js';
import { buildMediaWorkspaceRoutes } from './media-workspace-editor.routes.js';
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

	private _generateRoutes() {
		this._routes = buildMediaWorkspaceRoutes({
			variants: this.#variants ?? [],
			splitViewComponent: this._splitViewElement,
			splitView: this.#workspaceContext?.splitView,
			getIsForbidden: () => this.#isForbidden,
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
