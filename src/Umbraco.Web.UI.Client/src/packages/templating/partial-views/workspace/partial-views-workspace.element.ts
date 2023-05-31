import { UmbPartialViewsWorkspaceContext } from './partial-views-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute, IRoutingInfo, PageComponent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

import './partial-views-workspace-edit.element.js';

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
