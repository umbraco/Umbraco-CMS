import { UUITextStyles } from '@umbraco-ui/uui-css';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDocumentWorkspaceContext } from './document-workspace.context';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './document-workspace-editor.element';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDocumentWorkspaceContext(this);
	#element = document.createElement('umb-document-workspace-editor');

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId/:documentTypeKey',
			component: () => this.#element,
			setup: async (_component, info) => {
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
			setup: (_component, info) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}

	static styles = [UUITextStyles];
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
