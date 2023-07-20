import { UmbPartialViewsWorkspaceContext } from './partial-views-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

import './partial-views-workspace-edit.element.js';
import '../../components/insert-menu/templating-insert-menu.element.js';

import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-partial-views-workspace')
export class UmbPartialViewsWorkspaceElement extends UmbLitElement {
	#partialViewsWorkspaceContext = new UmbPartialViewsWorkspaceContext(this);

	#element = document.createElement('umb-partial-views-workspace-edit');

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey/:snippetName',
			component: () => this.#element,
			setup: async (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				const decodePath = decodeURIComponent(parentKey);
				const snippetName = info.match.params.snippetName;

				this.#partialViewsWorkspaceContext.create(decodePath === 'null' ? null : parentKey, snippetName);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#partialViewsWorkspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!
				);
			},
		},
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const key = info.match.params.key;
				const decodePath = decodeURIComponent(key).replace('-cshtml', '.cshtml');
				this.#partialViewsWorkspaceContext.load(decodePath);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbPartialViewsWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-views-workspace': UmbPartialViewsWorkspaceElement;
	}
}
