import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

/**
 * @element umb-property-editor-ui-block-grid-inner-test
 */
@customElement('umb-property-editor-ui-block-grid-inner-test')
export class UmbPropertyEditorUIBlockGridInnerTestElement extends UmbLitElement {
	@property({ type: String })
	public name = '';

	@state()
	private _routerPath: string | undefined;

	@state()
	private _activePath: string | undefined;

	@state()
	private _routes: UmbRoute[] = [
		{
			path: 'inner-1',
			component: () => {
				return import('./property-editor-ui-block-grid-inner-test.element.js');
			},
			setup: (component) => {
				if (component instanceof HTMLElement) {
					(component as any).name = 'inner-1';
				}
			},
		},
		{
			path: 'inner-2',
			component: () => {
				return import('./property-editor-ui-block-grid-inner-test.element.js');
			},
			setup: (component) => {
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

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIBlockGridInnerTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-inner-test': UmbPropertyEditorUIBlockGridInnerTestElement;
	}
}
