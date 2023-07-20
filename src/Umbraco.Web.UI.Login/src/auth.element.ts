import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import { ifDefined } from 'lit/directives/if-defined.js';

import './auth-layout.element.js';
import './pages/reset-password.element.js';
import './pages/new-password.element.js';
import './pages/login.element.js';
import { Router } from '@vaadin/router';
import { UmbAuthMainContext } from './context/auth-main.context.js';

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
	#returnPath = '';

	@property({ type: Boolean, attribute: 'is-legacy' })
	isLegacy = false;

	@property({ type: String, attribute: 'background-image' })
	backgroundImage = '';

	@property({ type: String, attribute: 'logo-image' })
	logoImage = '';

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = true;

	@property({ type: String, attribute: 'return-url' })
	set returnPath(value: string) {
		this.#returnPath = value;
		UmbAuthMainContext.Instance.returnPath = this.returnPath;
	}
	get returnPath() {
		// Check if there is a ?redir querystring or else return the returnUrl attribute
		return new URLSearchParams(window.location.search).get('returnPath') || this.#returnPath;
	}

	connectedCallback(): void {
		super.connectedCallback();

		requestAnimationFrame(() => {
			const router = new Router(this.shadowRoot?.getElementById('outlet'));

			router.setRoutes([
				{ path: '', component: 'umb-login' },
				{ path: 'reset', component: 'umb-reset-password' },
				{ path: 'new', component: 'umb-new-password' },
				{ path: '(.*)', redirect: '' },
			]);
		});
	}

	render() {
		return html`
			<umb-auth-layout backgroundImage=${ifDefined(this.backgroundImage)} logoImage=${ifDefined(this.logoImage)}>
				<div id="outlet"></div>
			</umb-auth-layout>
		`;
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth': UmbAuthElement;
	}
}
