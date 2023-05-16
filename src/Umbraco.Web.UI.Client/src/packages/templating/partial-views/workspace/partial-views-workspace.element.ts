import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbPartialViewsWorkspaceContext } from './partial-views-workspace.context';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './partial-views-workspace-edit.element';
import { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

@customElement('umb-partial-views-workspace')
export class UmbPartialViewsWorkspaceElement extends UmbLitElement {
	#partialViewsWorkspaceContext = new UmbPartialViewsWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-partial-views-workspace-edit');
	#key = '';

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				this.#partialViewsWorkspaceContext.createScaffold(parentKey);
			},
		},
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const key = info.match.params.key;
				this.#partialViewsWorkspaceContext.load(key);
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

	static styles = [UUITextStyles, css``];
}

export default UmbPartialViewsWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-views-workspace': UmbPartialViewsWorkspaceElement;
	}
}
