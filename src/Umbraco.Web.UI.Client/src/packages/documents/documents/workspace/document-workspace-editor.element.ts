import type { UmbDocumentVariantOptionModel } from '../types.js';
import { UmbDocumentWorkspaceSplitViewElement } from './document-workspace-split-view.element.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './context/document-workspace.context-token.js';
import { buildDocumentWorkspaceRoutes } from './document-workspace-editor.routes.js';
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
				const previousCulture = this.#appCulture;
				this.#appCulture = appCulture;
				this.#generateRoutes();
				this.#syncUrlToCulture(previousCulture, appCulture);
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

	#syncUrlToCulture(previousCulture: string | undefined, appCulture: string | undefined) {
		if (!previousCulture || !appCulture || previousCulture === appCulture || !this.#workspaceRoute) return;

		const currentPath = window.location.pathname;
		const routePrefix = this.#workspaceRoute + '/';

		if (!currentPath.startsWith(routePrefix)) return;

		const remainingPath = currentPath.substring(routePrefix.length);

		// Skip split-view paths
		if (remainingPath.includes('_&_')) return;

		// Separate the variant unique from any trailing path (e.g., /view/info)
		const slashIndex = remainingPath.indexOf('/');
		const variantUnique = slashIndex === -1 ? remainingPath : remainingPath.substring(0, slashIndex);
		const pathSuffix = slashIndex === -1 ? '' : remainingPath.substring(slashIndex);

		// Find the current variant to verify its culture matches the previous one
		const currentVariant = this.#variants?.find((v) => v.unique === variantUnique);
		if (currentVariant?.culture !== previousCulture) return;

		// Find the equivalent variant with the new culture, preserving the segment
		const newVariant = this.#variants?.find((v) => v.culture === appCulture && v.segment === currentVariant.segment);
		if (!newVariant) return;

		history.replaceState(
			null,
			'',
			`${this.#workspaceRoute}/${newVariant.unique}${pathSuffix}${window.location.search}`,
		);
	}

	#generateRoutes() {
		this._routes = buildDocumentWorkspaceRoutes({
			getVariants: () => this.#variants ?? [],
			getAppCulture: () => this.#appCulture,
			splitViewComponent: this._splitViewElement,
			splitView: this.#workspaceContext?.splitView,
			getWorkspaceRoute: () => this.#workspaceRoute,
			getIsForbidden: () => this.#isForbidden,
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
