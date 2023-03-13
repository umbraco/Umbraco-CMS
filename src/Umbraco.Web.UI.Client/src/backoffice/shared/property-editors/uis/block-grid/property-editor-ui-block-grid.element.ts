import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN } from '../../../../shared/components/workspace/workspace-variant/workspace-variant.context';
import { UmbVariantId } from '../../../../shared/variants/variant-id.class';
import { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/router';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	private _variantContext?: typeof UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN.TYPE;

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	@state()
	private _routes: IRoute[] = [];

	@state()
	private _routerPath: string | undefined;

	@state()
	private _activePath: string | undefined;

	@state()
	private _variantId?: UmbVariantId;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN, (context) => {
			this._variantContext = context;
			this.observe(this._variantContext?.variantId, (variantId) => {
				this._variantId = variantId;
				this.setupRoutes();
			});
		});
	}

	setupRoutes() {
		this._routes = [];
		if (this._variantId !== undefined) {
			this._routes = [
				{
					path: this._variantId.toString() + '/modal-1',
					component: () => {
						return import('./property-editor-ui-block-grid-inner-test.element');
					},
					setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
						console.log('block route', info);
						if (component instanceof HTMLElement) {
							(component as any).name = 'block-grid-1';
						}
					},
				},
				{
					path: this._variantId.toString() + '/modal-2',
					//pathMatch: 'full',
					component: () => {
						return import('./property-editor-ui-block-grid-inner-test.element');
					},
					setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
						console.log('block route', info);
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
							href="${this._routerPath + '/' + this._variantId.toString()}/modal-1"
							.active=${this._routerPath + '/' + this._variantId.toString() + '/modal-1' ===
							this._activePath}></uui-tab>
						<uui-tab
							label="TAB 2"
							href="${this._routerPath + '/' + this._variantId.toString()}/modal-2"
							.active=${this._routerPath + '/' + this._variantId.toString() + '/modal-2' ===
							this._activePath}></uui-tab>
					</uui-tab-group>

					<umb-router-slot
						id="router-slot"
						.routes="${this._routes}"
						@init=${(event: UmbRouterSlotInitEvent) => {
							this._routerPath = event.target.absoluteRouterPath;
						}}
						@change=${(event: UmbRouterSlotChangeEvent) => {
							this._activePath = event.target.localActiveViewPath;
						}}>
					</umb-router-slot>
			  </div>`
			: 'loading...';
	}
}

export default UmbPropertyEditorUIBlockGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid': UmbPropertyEditorUIBlockGridElement;
	}
}
