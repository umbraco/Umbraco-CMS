import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { IRoutingInfo, PageComponent, UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './template-workspace-editor.element.js';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	public load(entityId: string) {
		this.#templateWorkspaceContext.load(entityId);
	}

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-template-workspace-editor');

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;

				this.#templateWorkspaceContext.create(parentKey === 'root' ? null : parentKey);
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
