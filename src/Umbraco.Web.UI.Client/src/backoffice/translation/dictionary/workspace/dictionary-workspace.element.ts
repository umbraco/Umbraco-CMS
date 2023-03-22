import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbDictionaryWorkspaceContext } from './dictionary-workspace.context';
import { UmbDictionaryWorkspaceEditElement } from './dictionary-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dictionary-workspace')
export class UmbWorkspaceDictionaryElement extends UmbLitElement {
	static styles = [UUITextStyles];

	#workspaceContext = new UmbDictionaryWorkspaceContext(this);
	#element = new UmbDictionaryWorkspaceEditElement();

	@state()
	_routes: any[] = [
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
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbWorkspaceDictionaryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace': UmbWorkspaceDictionaryElement;
	}
}
