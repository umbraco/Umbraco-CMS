import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { Router } from '@vaadin/router';
import { UmbAuthMainContext } from './context/auth-main.context.js';

import './auth-layout.element.js';
import './pages/reset-password.element.js';
import './pages/new-password.element.js';
import './pages/login.element.js';

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

	async firstUpdated(): Promise<void> {
    // We need to find the value of the base tag to make sure the router works without trailing slash
    // even though the router normally finds the base tag automatically, but it does not seem to work
    // in this case where we want to use the router on /umbraco/login as well as /umbraco/login/
    const baseValue = document.querySelector('base')?.getAttribute('href') ?? '/umbraco/';
    const router = new Router(this.shadowRoot?.getElementById('outlet'), {
      baseUrl: baseValue.replace('login/', '') // Remove the "login/" part of the base url to make sure the router works without trailing slash
    });

    await router.setRoutes([
      { path: 'login', children: [
        { path: '', component: 'umb-login' },
        { path: 'reset', component: 'umb-reset-password' },
        { path: 'new', component: 'umb-new-password' },
        ] },
      { path: '(.*)', redirect: 'login' }
    ]);
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
