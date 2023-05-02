import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserWorkspaceContext } from './user-workspace.context';
import { UmbUserWorkspaceEditElement } from './user-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-user-workspace')
export class UmbUserWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbUserWorkspaceContext(this);
	#element = new UmbUserWorkspaceEditElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: ':id',
			component: () => this.#element,
			setup: (component, info) => {
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

export default UmbUserWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace': UmbUserWorkspaceElement;
	}
}
