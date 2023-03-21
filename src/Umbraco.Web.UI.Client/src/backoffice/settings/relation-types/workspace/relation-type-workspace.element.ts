import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRelationTypeWorkspaceContext } from './relation-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRouterSlotInitEvent, IRoute, IRoutingInfo } from '@umbraco-cms/internal/router';

import './relation-type-workspace-edit.element';

/**
 * @element umb-relation-type-workspace
 * @description - Element for displaying a Relation Type Workspace
 */
@customElement('umb-relation-type-workspace')
export class UmbRelationTypeWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#workspaceContext = new UmbRelationTypeWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-relation-type-workspace-edit-element');
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

export default UmbRelationTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-workspace': UmbRelationTypeWorkspaceElement;
	}
}
