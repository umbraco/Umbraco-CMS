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
import './pages/invite.element.js';
import './external-login-providers-layout.element.js';
import { InterfaceLookValues } from '@umbraco-ui/uui';

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
	set usernameIsEmail(value: boolean) {
		UmbAuthMainContext.Instance.usernameIsEmail = value;
	}
	get usernameIsEmail() {
		return UmbAuthMainContext.Instance.usernameIsEmail;
	}
	@property({ type: Boolean, attribute: 'allow-password-reset' })
	set allowPasswordReset(value: boolean) {
		UmbAuthMainContext.Instance.allowPasswordReset = value;
	}
	get allowPasswordReset() {
		return UmbAuthMainContext.Instance.allowPasswordReset;
	}

	@property({ type: Boolean, attribute: 'allow-user-invite' })
	allowUserInvite = false;

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
					{ path: '', component: 'umb-login', action: this.#checkResetCode },
					{ path: 'reset', component: 'umb-reset-password', action: this.#checkRouteAllowReset },
					{ path: 'new', component: 'umb-new-password', action: this.#checkRouteAllowReset },
					{ path: 'invite', component: 'umb-invite', action: this.#checkRouteAllowInvite },
					{ path: '(.*)', redirect: '' },
				],
			},
			{ path: '(.*)', redirect: 'login' },
		]);
	}

	#checkRouteAllowReset = (_context: Context, commands: Commands) => {
		if (!this.allowPasswordReset) {
			return commands.redirect('login');
		}
	};

	#checkRouteAllowInvite = (_context: Context, commands: Commands) => {
		if (!this.allowUserInvite) {
			return commands.redirect('login');
		}
	};

	#checkResetCode = (_context: Context, _commands: Commands) => {
		//TODO: You should be able to use the router to redirect, but the commands.redirect() doesn't work with params.
		//TODO: And the Router.go() doesn't work at all for some reason.
		const flow = new URLSearchParams(window.location.search).get('flow');
		const status = new URLSearchParams(window.location.search).get('status');

		if (flow === 'reset-password' && status === 'resetCodeExpired') {
			window.history.replaceState({}, '', 'login/reset?status=resetCodeExpired');
			window.history.go(0);
		}
	};

	render() {
		return html`
			<umb-auth-layout backgroundImage=${ifDefined(this.backgroundImage)} logoImage=${ifDefined(this.logoImage)}>
				<div id="outlet"></div>
				<umb-external-login-providers-layout>
					<umb-login-external
						.options=${{
							name: 'Google',
							buttonLook: 'outline',
							buttonColor: 'danger',
							icon: 'info',
						}}></umb-login-external>
				</umb-external-login-providers-layout>
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
