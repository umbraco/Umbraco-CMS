import { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { generateRoutePathBuilder } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './language-workspace-editor.element.js';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);

	/**
	 * Workspace editor element, lazy loaded but shared across several user flows.
	 */
	#editorElement?: HTMLElement;

	#routerPath? = '';

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

				// Navigate to edit route when language is created:
				this.observe(
					this.#languageWorkspaceContext.isNew,
					(isNew) => {
						if (isNew === false) {
							const isoCode = this.#languageWorkspaceContext.getEntityId();
							if (this.#routerPath && isoCode) {
								const routeBasePath = this.#routerPath.endsWith('/') ? this.#routerPath : this.#routerPath + '/';
								// TODO: Revisit if this is the right way to change URL:
								const newPath = generateRoutePathBuilder(routeBasePath + 'edit/:isoCode')({ isoCode });
								window.history.pushState({}, '', newPath);
							}
						}
					},
					'_observeIsNew'
				);
			},
		},
	];

	render() {
		return html`<umb-router-slot
			.routes=${this._routes}
			@init=${(event: UmbRouterSlotInitEvent) => {
				this.#routerPath = event.target.absoluteRouterPath;
			}}></umb-router-slot>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
