import { UUITextStyles } from '@umbraco-ui/uui-css';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDocumentTypeWorkspaceContext } from './document-type-workspace.context';
import { UmbDocumentTypeWorkspaceEditorElement } from './document-type-workspace-editor.element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-type-workspace')
export class UmbDocumentTypeWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbDocumentTypeWorkspaceContext(this);
	#element = new UmbDocumentTypeWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentId',
			component: () => this.#element,
			setup: (_component, info) => {
				const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
				this.#workspaceContext.createScaffold(parentId);
			},
		},
		{
			path: 'edit/:id',
			component: () => this.#element,
			setup: (_component, info) => {
				console.log('Setup for edit/id');
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [UUITextStyles];
}

export default UmbDocumentTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace': UmbDocumentTypeWorkspaceElement;
	}
}
