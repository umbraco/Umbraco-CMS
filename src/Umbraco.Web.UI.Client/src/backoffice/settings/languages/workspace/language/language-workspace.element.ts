import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoutingInfo } from 'router-slot';
import { UmbLanguageWorkspaceContext } from './language-workspace.context';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);

	#routerPath? = '';

	// TODO: add create route
	@state()
	_routes = [
		{
			path: 'edit/:isoCode',
			component: () => import('./language-workspace-edit.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				this.#languageWorkspaceContext.load(info.match.params.isoCode);
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
