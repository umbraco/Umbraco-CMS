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
	private splitViewElement = new UmbMemberWorkspaceSplitViewElement();

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	@state()
	_routes?: Array<UmbRoute>;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (instance) => {
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

	private async _generateRoutes(options: Array<UmbMemberVariantOptionModel>) {
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

		routes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});

		this._routes = routes;
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceContext?.splitView.setWorkspaceRoute(e.target.absoluteRouterPath);
	};

	override render() {
		return this._routes && this._routes.length > 0
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
