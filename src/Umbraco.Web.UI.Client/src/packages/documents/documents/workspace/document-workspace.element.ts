import { UmbDocumentWorkspaceContext } from './document-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './document-workspace-editor.element.js';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDocumentWorkspaceContext(this);

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId/:documentTypeKey',
			component: import('./document-workspace-editor.element.js'),
			setup: async (_component, info) => {
				// TODO: use parent id:
				// TODO: Notice the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				const documentTypeKey = info.match.params.documentTypeKey;
				this.#workspaceContext.create(documentTypeKey, parentId);
			},
		},
		{
			path: 'edit/:id',
			component: import('./document-workspace-editor.element.js'),
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
