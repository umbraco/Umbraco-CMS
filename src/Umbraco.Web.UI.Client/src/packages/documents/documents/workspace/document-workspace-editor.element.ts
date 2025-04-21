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
	private splitViewElement = new UmbDocumentWorkspaceSplitViewElement();

	#appLanguage?: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	#workspaceRoute?: string;
	#appCulture?: string;
	#variants?: Array<UmbDocumentVariantOptionModel>;

	@state()
	_routes?: Array<UmbRoute>;

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
		});
	}

	#handleVariantFolderPart(index: number, folderPart: string) {
		const variantSplit = folderPart.split('_');
		const culture = variantSplit[0];
		const segment = variantSplit[1];
		this.#workspaceContext?.splitView.setActiveVariant(index, culture, segment);
	}

	#generateRoutes() {
		if (!this.#variants || !this.#appCulture) return;

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		this.#variants.forEach((variantA) => {
			this.#variants!.forEach((variantB) => {
				routes.push({
					// TODO: When implementing Segments, be aware if using the unique still is URL Safe, cause its most likely not... [NL]
					path: variantA.unique + '_&_' + variantB.unique,
					component: this.splitViewElement,
					setup: (_component, info) => {
						// Set split view/active info..
						const variantSplit = info.match.fragments.consumed.split('_&_');
						variantSplit.forEach((part, index) => {
							this.#handleVariantFolderPart(index, part);
						});
					},
				});
			});
		});

		// Single view:
		this.#variants.forEach((variant) => {
			routes.push({
				// TODO: When implementing Segments, be aware if using the unique still is URL Safe, cause its most likely not... [NL]
				path: variant.unique,
				component: this.splitViewElement,
				setup: (_component, info) => {
					// cause we might come from a split-view, we need to reset index 1.
					this.#workspaceContext?.splitView.removeActiveVariant(1);
					this.#handleVariantFolderPart(0, info.match.fragments.consumed);
				},
			});
		});

		if (routes.length !== 0) {
			// Using first single view as the default route for now (hence the math below):
			routes.push({
				path: '',
				resolve: () => {
					const route = routes.find((route) => route.path === this.#appCulture);

					if (!route) {
						// TODO: Notice: here is a specific index used for fallback, this could be made more solid [NL]
						history.replaceState({}, '', `${this.#workspaceRoute}/${routes[routes.length - 3].path}`);
						return;
					}

					history.replaceState({}, '', `${this.#workspaceRoute}/${route?.path}`);
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

export default UmbDocumentWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-editor': UmbDocumentWorkspaceEditorElement;
	}
}
