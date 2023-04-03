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

	#element = document.createElement('umb-data-type-workspace-edit-element');

	@state()
	_routes: IRoute[] = [
		{
			path: 'create/:parentId',
			component: () => this.#element,
			setup: async (component: HTMLElement, info: IRoutingInfo) => {
				const parentId = info.match.params.parentId;
				this.#workspaceContext.createScaffold(parentId);
			},
		},
		{
			path: 'edit/:id',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
