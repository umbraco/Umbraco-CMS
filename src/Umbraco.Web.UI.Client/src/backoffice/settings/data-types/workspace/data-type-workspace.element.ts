import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context';
import { UmbRouterSlotInitEvent, IRoute, IRoutingInfo } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './data-type-workspace-edit.element';

@customElement('umb-data-type-workspace')
export class UmbDataTypeWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#workspaceContext = new UmbDataTypeWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-data-type-workspace-edit-element');
	#key = '';

	@state()
	_routes: IRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: async (component: HTMLElement, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				this.#workspaceContext.createScaffold(parentKey);
			},
		},
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const key = info.match.params.key;
				this.#workspaceContext.load(key);
			},
		},
	];

	render() {
		return html`<umb-router-slot
			.routes=${this._routes}
			@init=${(event: UmbRouterSlotInitEvent) => {
				this.#routerPath = event.target.absoluteRouterPath;
			}}></umb-router-slot>`;
	}
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
