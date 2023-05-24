import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN } from '../../../workspace/workspace-variant/workspace-variant.context.js';
import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '../../../workspace/workspace-property/workspace-property.context.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	private _variantContext?: typeof UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN.TYPE;

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config?: UmbDataTypePropertyCollection;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	private _routerPath: string | undefined;

	@state()
	private _activePath: string | undefined;

	@state()
	private _variantId?: UmbVariantId;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, (context) => {
			this.observe(context?.variantId, (propertyVariantId) => {
				this._variantId = propertyVariantId;
				this.setupRoutes();
			});
		});
	}

	setupRoutes() {
		this._routes = [];
		if (this._variantId !== undefined) {
			this._routes = [
				{
					path: 'modal-1',
					component: () => {
						return import('./property-editor-ui-block-grid-inner-test.element');
					},
					setup: (component) => {
						if (component instanceof HTMLElement) {
							(component as any).name = 'block-grid-1';
						}
					},
				},
				{
					path: 'modal-2',
					component: () => {
						return import('./property-editor-ui-block-grid-inner-test.element');
					},
					setup: (component) => {
						if (component instanceof HTMLElement) {
							(component as any).name = 'block-grid-2';
						}
					},
				},
			];
		}
	}

	render() {
		return this._variantId
			? html`<div>
					umb-property-editor-ui-block-grid, inner routing test:

					<uui-tab-group slot="tabs">
						<uui-tab
							label="TAB 1"
							href="${this._routerPath + '/'}modal-1"
							.active=${this._routerPath + '/' + 'modal-1' === this._activePath}></uui-tab>
						<uui-tab
							label="TAB 2"
							href="${this._routerPath + '/'}modal-2"
							.active=${this._routerPath + '/' + 'modal-2' === this._activePath}></uui-tab>
					</uui-tab-group>

					<umb-variant-router-slot
						.variantId=${[this._variantId]}
						id="router-slot"
						.routes="${this._routes}"
						@init=${(event: UmbRouterSlotInitEvent) => {
							this._routerPath = event.target.absoluteRouterPath;
						}}
						@change=${(event: UmbRouterSlotChangeEvent) => {
							this._activePath = event.target.localActiveViewPath;
						}}>
					</umb-variant-router-slot>
			  </div>`
			: 'loading...';
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIBlockGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid': UmbPropertyEditorUIBlockGridElement;
	}
}
