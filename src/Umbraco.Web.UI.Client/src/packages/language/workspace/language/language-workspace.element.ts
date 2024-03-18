import { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UmbLanguageWorkspaceEditorElement } from './language-workspace-editor.element.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbLanguageWorkspaceContext(this);
	#createElement = () => new UmbLanguageWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create',
			component: this.#createElement,
			setup: async () => {
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
			setup: (_component, info) => {
				this.removeControllerByAlias('isNewRedirectController');
				this.#workspaceContext.load(info.match.params.unique);
			},
		},
	];

	constructor() {
		super();
		// TODO: We need to recreate when ID changed?
		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [this, this.#workspaceContext]);
	}

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
