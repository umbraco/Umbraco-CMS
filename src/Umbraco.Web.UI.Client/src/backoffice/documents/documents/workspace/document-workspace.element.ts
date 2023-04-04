import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo } from 'router-slot';
import { UmbDocumentWorkspaceContext } from './document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './document-workspace-editor.element';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles];

	#workspaceContext = new UmbDocumentWorkspaceContext(this);
	#element = document.createElement('umb-document-workspace-editor');

	@state()
	_routes: IRoute[] = [
		{
			path: 'create/:parentKey/:documentTypeKey',
			component: () => this.#element,
			setup: async (component: HTMLElement, info: IRoutingInfo) => {
				// TODO: use parent key:
				// TODO: Notice the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
				const parentKey = info.match.params.parentKey;
				const documentTypeKey = info.match.params.documentTypeKey;
				this.#workspaceContext.createScaffold(documentTypeKey);
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
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
