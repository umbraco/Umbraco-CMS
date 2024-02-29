import type { UmbDocumentVariantOptionModel } from '../types.js';
import { UmbDocumentWorkspaceSplitViewElement } from './document-workspace-split-view.element.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

// TODO: This seem fully identical with Media Workspace Editor, so we can refactor this to a generic component. [NL]
@customElement('umb-document-workspace-editor')
export class UmbDocumentWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private splitViewElement = new UmbDocumentWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_routes?: Array<UmbRoute>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
		});
	}

	#observeVariants() {
		if (!this.#workspaceContext) return;
		// TODO: the variantOptions observable is like too broad as this will be triggered then there is any change in the variant options, we need to only update routes when there is a relevant change to them. [NL]
		this.observe(this.#workspaceContext.variantOptions, (options) => this._generateRoutes(options), '_observeVariants');
	}

	private _handleVariantFolderPart(index: number, folderPart: string) {
		const variantSplit = folderPart.split('_');
		const culture = variantSplit[0];
		const segment = variantSplit[1];
		this.#workspaceContext?.splitView.setActiveVariant(index, culture, segment);
	}

	private async _generateRoutes(options: Array<UmbDocumentVariantOptionModel>) {
		if (!options || options.length === 0) return;

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		// Split view routes:
		options.forEach((variantA) => {
			options.forEach((variantB) => {
				routes.push({
					// TODO: When implementing Segments, be aware if using the unique is URL Safe... [NL]
					path: variantA.unique + '_&_' + variantB.unique,
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
		options.forEach((variant) => {
			routes.push({
				// TODO: When implementing Segments, be aware if using the unique is URL Safe... [NL]
				path: variant.unique,
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
				redirectTo: routes[options.length * options.length]?.path,
			});
		}

		const oldValue = this._routes;

		// is there any differences in the amount ot the paths? [NL]
		// TODO: if we make a memorization function as the observer, we can avoid this check and avoid the whole build of routes. [NL]
		if (oldValue && oldValue.length === routes.length) {
			// is there any differences in the paths? [NL]
			const hasDifferences = oldValue.some((route, index) => route.path !== routes[index].path);
			if (!hasDifferences) return;
		}
		this._routes = routes;
		this.requestUpdate('_routes', oldValue);
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceContext?.splitView.setWorkspaceRoute(e.target.absoluteRouterPath);
	};

	render() {
		return this._routes && this._routes.length > 0
			? html`<umb-router-slot .routes=${this._routes} @init=${this._gotWorkspaceRoute}></umb-router-slot>`
			: '';
	}

	static styles = [
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
