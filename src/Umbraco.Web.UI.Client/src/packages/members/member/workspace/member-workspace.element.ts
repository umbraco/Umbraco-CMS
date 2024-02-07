import { UmbMemberWorkspaceContext } from './member-workspace.context.js';
import { UmbMemberWorkspaceEditorElement } from './member-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-member-workspace')
export class UmbMemberWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbMemberWorkspaceContext(this);
	#createElement = () => new UmbMemberWorkspaceEditorElement();

	private _routes: UmbRoute[] = [
		{
			path: 'create/:memberTypeUnique',
			component: this.#createElement,
			setup: (_component, info) => {
				const memberTypeUnique = info.match.params.memberTypeUnique;
				this.#workspaceContext.create(null, memberTypeUnique);

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
			setup: (_component, info) => {
				const unique = info.match.params.unique;
				this.#workspaceContext.load(unique);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbMemberWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace': UmbMemberWorkspaceElement;
	}
}
