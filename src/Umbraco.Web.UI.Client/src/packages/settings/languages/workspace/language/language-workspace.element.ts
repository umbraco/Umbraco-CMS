import { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './language-workspace-editor.element.js';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);

	/**
	 * Workspace editor element, lazy loaded but shared across several user flows.
	 */
	#editorElement?: HTMLElement;

	#getComponentElement = async () => {
		if (this.#editorElement) {
			return this.#editorElement;
		}
		this.#editorElement = new (await import('./language-workspace-editor.element.js')).default();
		return this.#editorElement;
	};

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'edit/:isoCode',
			component: this.#getComponentElement,
			setup: (_component, info) => {
				this.removeControllerByAlias('_observeIsNew');
				this.#languageWorkspaceContext.load(info.match.params.isoCode);
			},
		},
		{
			path: 'create',
			component: this.#getComponentElement,
			setup: async () => {
				this.#languageWorkspaceContext.create();

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#languageWorkspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!
				);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
