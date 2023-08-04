import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbAuthMainContext } from './context/auth-main.context.js';
import { UUIIconRegistryEssential } from '@umbraco-ui/uui';
import { UmbIconRegistry } from './icon.registry.js';
import UmbRouter from './umb-router.js';

import './auth-layout.element.js';
import './pages/reset-password.element.js';
import './pages/new-password.element.js';
import './pages/login.element.js';
import './pages/invite.element.js';

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
	#returnPath = '';

	@property({ type: Boolean, attribute: 'is-legacy' })
	isLegacy = false;

	@property({ type: Boolean, attribute: 'disable-local-login' })
	disableLocalLogin = false;

	@property({ type: String, attribute: 'background-image' })
	backgroundImage = '';

	@property({ type: String, attribute: 'logo-image' })
	logoImage = '';

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = false;

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

	constructor() {
		super();

		new UUIIconRegistryEssential().attach(this);
		new UmbIconRegistry().attach(this);
	}

	@state()
	router?: UmbRouter;

	async firstUpdated(): Promise<void> {
		this.router = new UmbRouter(this, [
			{
				path: 'login',
				component: html`<umb-login
					?allow-password-reset=${this.allowPasswordReset}
					?username-is-email=${this.usernameIsEmail}>
					<slot name="external" slot="external"></slot>
				</umb-login>`,
				default: true,
				action: this.#checkRouteForResetParams,
			},
			{
				path: 'login/reset',
				component: html`<umb-reset-password></umb-reset-password>`,
				action: () => (this.allowPasswordReset ? null : 'login'),
			},
			{
				path: 'login/new',
				component: html`<umb-new-password></umb-new-password>`,
				action: () => (this.allowPasswordReset ? null : 'login'),
			},
			{
				path: 'login/invite',
				component: html`<umb-invite></umb-invite>`,
				action: () => (this.allowUserInvite ? null : 'login'),
			},
		]);

		this.router.subscribe();
	}

	#checkRouteForResetParams(_path: string, search: string) {
		const searchParams = new URLSearchParams(search);
		const flow = searchParams.get('flow');
		const resetId = searchParams.get('userId');
		const resetCode = searchParams.get('resetCode');

		console.log(flow, resetId, resetCode);

		if (flow === 'reset-password' && resetId && resetCode) {
			return 'login/new';
		}

		return null;
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.router?.unsubscribe();
	}

	render() {
		return html`
			<umb-auth-layout backgroundImage=${ifDefined(this.backgroundImage)} logoImage=${ifDefined(this.logoImage)}>
				${this.router?.render()}
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
