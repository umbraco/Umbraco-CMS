import { UmbDocumentWorkspaceSplitViewElement } from './document-workspace-split-view.element.js';
import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { VariantModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT, ActiveVariant } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-document-workspace-editor')
export class UmbDocumentWorkspaceEditorElement extends UmbLitElement {
	//private _defaultVariant?: VariantViewModelBaseModel;
	private splitViewElement = new UmbDocumentWorkspaceSplitViewElement();

	@state()
	_unique?: string;

	@state()
	_routes?: Array<UmbRoute>;

	@state()
	_availableVariants: Array<VariantModelBaseModel> = [];

	@state()
	_workspaceSplitViews: Array<ActiveVariant> = [];

	#workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbDocumentWorkspaceContext;
			this.#observeVariants();
			this.#observeSplitViews();
		});
	}

	#observeVariants() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.variants, (variants) => {
			this._availableVariants = variants;
			this._generateRoutes();
		});
	}

	#observeSplitViews() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.splitView.activeVariantsInfo, (variants) => {
			this._workspaceSplitViews = variants;
		});
	}

	private _handleVariantFolderPart(index: number, folderPart: string) {
		const variantSplit = folderPart.split('_');
		const culture = variantSplit[0];
		const segment = variantSplit[1];
		this.#workspaceContext?.splitView.setActiveVariant(index, culture, segment);
	}

	private _generateRoutes() {
		if (!this._availableVariants || this._availableVariants.length === 0) return;

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		this._availableVariants.forEach((variantA) => {
			this._availableVariants.forEach((variantB) => {
				routes.push({
					path: new UmbVariantId(variantA).toString() + '_&_' + new UmbVariantId(variantB).toString(),
					component: this.splitViewElement,
					setup: (_component, info) => {
						// Set split view/active info..
						const variantSplit = info.match.fragments.consumed.split('_&_');
						variantSplit.forEach((part, index) => {
							this._handleVariantFolderPart(index, part);
						});
					},
				});
			});
		});

		// Single view:
		this._availableVariants.forEach((variant) => {
			routes.push({
				path: new UmbVariantId(variant).toString(),
				component: this.splitViewElement,
				setup: (_component, info) => {
					// cause we might come from a split-view, we need to reset index 1.
					this.#workspaceContext?.splitView.removeActiveVariant(1);
					this._handleVariantFolderPart(0, info.match.fragments.consumed);
				},
			});
		});

		if (routes.length !== 0) {
			// Using first single view as the default route for now (hence the math below):
			routes.push({
				path: '',
				redirectTo: routes[this._availableVariants.length * this._availableVariants.length]?.path,
			});
		}

		this._routes = routes;
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceContext?.splitView.setWorkspaceRoute(e.target.absoluteRouterPath);
	};

	render() {
		return this._routes
			? html`<umb-router-slot .routes=${this._routes} @init=${this._gotWorkspaceRoute}
					>${this.splitViewElement}</umb-router-slot
			  >`
			: '';
	}

	static styles = [
		UUITextStyles,
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
