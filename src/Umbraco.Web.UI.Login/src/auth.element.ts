import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

import './auth-layout.element.js';
import './reset-password.element.js';
import './new-password.element.js';
import './login.element.js';
import { Router } from '@vaadin/router';

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
	#returnUrl = '';

	@property({ type: Boolean, attribute: 'is-legacy' })
	isLegacy = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = true;

	@property({ type: String, attribute: 'return-url' })
	set returnUrl(value: string) {
		this.#returnUrl = value;
	}

	get returnUrl() {
		// Check if there is a ?redir querystring or else return the returnUrl attribute
		return new URLSearchParams(window.location.search).get('returnPath') || this.#returnUrl;
	}

	connectedCallback(): void {
		super.connectedCallback();

		requestAnimationFrame(() => {
			const router = new Router(this.shadowRoot?.getElementById('outlet'));
			router.setRoutes([
				{ path: '', component: 'umb-login' },
				{ path: '/reset', component: 'umb-reset-password' },
				{ path: '/new', component: 'umb-new-password' },
				{ path: '(.*)', redirect: '' },
			]);
		});
	}

	render() {
		return html`
			<umb-auth-layout>
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
