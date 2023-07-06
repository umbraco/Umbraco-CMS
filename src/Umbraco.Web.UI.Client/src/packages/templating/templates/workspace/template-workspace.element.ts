import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { IRoutingInfo, PageComponent, UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../../components/insert-menu/templating-insert-menu.element.js';
import './template-workspace-editor.element.js';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	public load(entityId: string) {
		this.#templateWorkspaceContext.load(entityId);
	}

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);

	#element = document.createElement('umb-template-workspace-editor');

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#templateWorkspaceContext.create(parentId);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#templateWorkspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!
				);
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
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbTemplateWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace': UmbTemplateWorkspaceElement;
	}
}
