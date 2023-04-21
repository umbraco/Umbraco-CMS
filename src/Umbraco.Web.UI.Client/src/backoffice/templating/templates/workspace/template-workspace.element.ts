import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import type { IRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './template-workspace-edit.element';
import { UmbTemplateWorkspaceContext } from './template-workspace.context';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-template-workspace-edit');
	#key = '';

	@state()
	_routes: IRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				this.#templateWorkspaceContext.createScaffold(parentKey);
			},
		},
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo): void => {
				const key = info.match.params.key;
				this.#templateWorkspaceContext.load(key);
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

export default UmbTemplateWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace': UmbTemplateWorkspaceElement;
	}
}
