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
			path: 'create/:parentId/:documentTypeKey',
			component: () => this.#element,
			setup: async (component: HTMLElement, info: IRoutingInfo) => {
				// TODO: use parent id:
				// TODO: Notice the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
				const parentId = info.match.params.parentId;
				const documentTypeKey = info.match.params.documentTypeKey;
				this.#workspaceContext.createScaffold(documentTypeKey);
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
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
