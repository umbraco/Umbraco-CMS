import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbLanguageWorkspaceContext } from './language-workspace.context';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './language-workspace-edit.element';
import { generateRoutePathBuilder } from '@umbraco-cms/backoffice/router';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);
	#element = document.createElement('umb-language-workspace-edit');

	#routerPath? = '';

	@state()
	_routes = [
		{
			path: 'edit/:isoCode',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				this.removeControllerByUnique('_observeIsNew');
				this.#languageWorkspaceContext.load(info.match.params.isoCode);
			},
		},
		{
			path: 'create',
			component: () => this.#element,
			setup: async () => {
				this.#languageWorkspaceContext.createScaffold();

				// Navigate to edit route when language is created:
				this.observe(
					this.#languageWorkspaceContext.isNew,
					(isNew) => {
						console.log('observe', isNew);
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
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
