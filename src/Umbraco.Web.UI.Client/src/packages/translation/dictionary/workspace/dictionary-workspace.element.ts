import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDictionaryWorkspaceContext } from './dictionary-workspace.context.js';
import { UmbDictionaryWorkspaceEditElement } from './dictionary-workspace-edit.element.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dictionary-workspace')
export class UmbWorkspaceDictionaryElement extends UmbLitElement {
	#workspaceContext = new UmbDictionaryWorkspaceContext(this);
	#element = new UmbDictionaryWorkspaceEditElement();

	@state()
	_routes: UmbRoute[] = [
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
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [UUITextStyles];
}

export default UmbWorkspaceDictionaryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace': UmbWorkspaceDictionaryElement;
	}
}
