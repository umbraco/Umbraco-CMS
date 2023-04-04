import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLanguageWorkspaceContext } from './language-workspace.context';
import type { IRoute } from '@umbraco-cms/backoffice/router';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './language-workspace-edit.element';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);
	#element = document.createElement('umb-language-workspace-edit');

	#routerPath? = '';

	@state()
	_routes: IRoute[] = [
		{
			path: 'edit/:isoCode',
			component: () => this.#element,
			setup: (_component, info) => {
				this.#languageWorkspaceContext.load(info.match.params.isoCode);
			},
		},
		{
			path: 'create',
			component: () => this.#element,
			setup: () => {
				this.#languageWorkspaceContext.createScaffold();
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
