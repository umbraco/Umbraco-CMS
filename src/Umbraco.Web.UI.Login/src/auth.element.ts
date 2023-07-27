import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { Commands, Context, Router } from '@vaadin/router';
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

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = true;

	@property({ type: Boolean, attribute: 'allow-user-invite' })
	allowUserInvite = true;

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
		const router = new Router(this.shadowRoot?.getElementById('outlet'));

		await router.setRoutes([
			{
				path: 'login',
				children: [
					{ path: '', component: 'umb-login', action: this.#test },
					{ path: 'reset', component: 'umb-reset-password' },
					{ path: 'new', component: 'umb-new-password' },
				],
			},
			{ path: '(.*)', redirect: 'login' },
		]);
	}

	#test(context: Context, commands: Commands) {
		//TODO: You should be able to use the router to redirect, but the commands.redirect() doesn't work with params.
		const flow = new URLSearchParams(window.location.search).get('flow');
		const status = new URLSearchParams(window.location.search).get('status');

		if (flow === 'reset-password' && status === 'resetCodeExpired') {
			window.history.replaceState({}, '', 'login/reset?status=resetCodeExpired');
			window.history.go(0);
		}
	}

	render() {
		return html`
			<umb-auth-layout .backgroundImage=${ifDefined(this.backgroundImage)} .logoImage=${ifDefined(this.logoImage)}>
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
