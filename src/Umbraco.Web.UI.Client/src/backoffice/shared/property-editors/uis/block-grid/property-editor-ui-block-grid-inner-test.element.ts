import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/router';

/**
 * @element umb-property-editor-ui-block-grid-inner-test
 */
@customElement('umb-property-editor-ui-block-grid-inner-test')
export class UmbPropertyEditorUIBlockGridInnerTestElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property({ type: String })
	public name = '';

	@state()
	private _routerPath: string | undefined;

	@state()
	private _activePath: string | undefined;

	@state()
	private _routes: IRoute[] = [
		{
			path: 'inner-1',
			component: () => {
				return import('./property-editor-ui-block-grid-inner-test.element');
			},
			setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
				console.log('block route inner', info);
				if (component instanceof HTMLElement) {
					(component as any).name = 'inner-1';
				}
			},
		},
		{
			path: 'inner-2',
			//pathMatch: 'full',
			component: () => {
				return import('./property-editor-ui-block-grid-inner-test.element');
			},
			setup: (component: Promise<HTMLElement> | HTMLElement, info: IRoutingInfo) => {
				console.log('block route inner', info);
				if (component instanceof HTMLElement) {
					(component as any).name = 'inner-2';
				}
			},
		},
	];

	render() {
		return html`<div>
			inner: ${this.name}

			<uui-tab-group slot="tabs">
				<uui-tab
					label="INNER TAB 1"
					href="${this._routerPath}/inner-1"
					.active=${this._routerPath + '/inner-1' === this._activePath}></uui-tab>
				<uui-tab
					label="INNER TAB 2"
					href="${this._routerPath}/inner-2"
					.active=${this._routerPath + '/inner-2' === this._activePath}></uui-tab>
			</uui-tab-group>

			<umb-router-slot
				id="router-slot"
				.routes="${this._routes}"
				@init=${(event: UmbRouterSlotInitEvent) => {
					this._routerPath = event.target.absoluteRouterPath;
				}}
				@change=${(event: UmbRouterSlotChangeEvent) => {
					this._activePath = event.target.localActiveViewPath;
				}}></umb-router-slot>
		</div>`;
	}
}

export default UmbPropertyEditorUIBlockGridInnerTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-inner-test': UmbPropertyEditorUIBlockGridInnerTestElement;
	}
}
