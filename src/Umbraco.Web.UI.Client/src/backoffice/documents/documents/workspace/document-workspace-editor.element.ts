import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbVariantId } from '../../../shared/variants/variant-id.class';
import { ActiveVariant } from '../../../shared/components/workspace/workspace-context/workspace-split-view-manager.class';
import { UmbDocumentWorkspaceContext } from './document-workspace.context';
import { UmbDocumentWorkspaceSplitViewElement } from './document-workspace-split-view.element';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../../../shared/components/workspace/workspace-variant/workspace-variant.element';
import { VariantModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-workspace-editor')
export class UmbDocumentWorkspaceEditorElement extends UmbLitElement {
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

	//private _defaultVariant?: VariantViewModelBaseModel;
	private splitViewElement = new UmbDocumentWorkspaceSplitViewElement();

	@state()
	_unique?: string;

	@state()
	_routes?: Array<IRoute>;

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
		const routes: Array<IRoute> = [];

		// Split view routes:
		this._availableVariants.forEach((variantA) => {
			this._availableVariants.forEach((variantB) => {
				routes.push({
					path: new UmbVariantId(variantA).toString() + '_&_' + new UmbVariantId(variantB).toString(),
					component: this.splitViewElement,
					setup: (component: HTMLElement | Promise<HTMLElement>, info: IRoutingInfo) => {
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
				setup: (component: HTMLElement | Promise<HTMLElement>, info: IRoutingInfo) => {
					// cause we might come from a split-view, we need to reset index 1.
					this.#workspaceContext?.splitView.removeActiveVariant(1);
					this._handleVariantFolderPart(0, info.match.fragments.consumed);
				},
			});
		});

		if (routes.length !== 0) {
			// Using first single view as the default route for now (hence the math below):
			routes.push({
				path: '**',
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
}

export default UmbDocumentWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-editor': UmbDocumentWorkspaceEditorElement;
	}
}
