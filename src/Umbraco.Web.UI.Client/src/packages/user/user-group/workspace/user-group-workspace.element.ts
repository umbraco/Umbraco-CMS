import { UmbUserGroupWorkspaceContext } from './user-group-workspace.context.js';
import { UmbUserGroupWorkspaceEditorElement } from './user-group-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-user-group-workspace')
export class UmbUserGroupWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbUserGroupWorkspaceContext(this);
	#createElement = () => new UmbUserGroupWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create',
			component: this.#createElement,
			setup: (component, info) => {
				this.#workspaceContext.create();

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:unique',
			component: this.#createElement,
			setup: (component, info) => {
				const unique = info.match.params.unique;
				this.#workspaceContext.load(unique);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [UmbTextStyles];
}

export default UmbUserGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace': UmbUserGroupWorkspaceElement;
	}
}
