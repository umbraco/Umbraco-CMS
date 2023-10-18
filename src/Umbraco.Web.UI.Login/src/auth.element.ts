import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from './context/auth.context.js';
import UmbRouter from './utils/umb-router.js';
import { umbLocalizationContext } from './external/localization/localization-context.js';

@customElement('umb-auth')
export default class UmbAuthElement extends LitElement {
	#returnPath = '';

	/**
	 * Disables the local login form and only allows external login providers.
	 *
	 * @attr disable-local-login
	 */
	@property({ type: Boolean, attribute: 'disable-local-login' })
	set disableLocalLogin(value: boolean) {
		umbAuthContext.disableLocalLogin = value;
	}

	@property({ type: String, attribute: 'background-image' })
	backgroundImage?: string;

	@property({ type: String, attribute: 'logo-image' })
	logoImage?: string;

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = false;

	@property({ type: Boolean, attribute: 'allow-user-invite' })
	allowUserInvite = false;

	@property({ type: String, attribute: 'return-url' })
	set returnPath(value: string) {
		this.#returnPath = value;
		umbAuthContext.returnPath = this.returnPath;
	}
	get returnPath() {
		// Check if there is a ?redir querystring or else return the returnUrl attribute
		return new URLSearchParams(window.location.search).get('returnPath') || this.#returnPath;
	}

	@state()
	protected router?: UmbRouter;

	/**
	 * Override the default flow.
	 */
	protected flow?: 'mfa' | 'reset-password' | 'invite-user';

	constructor() {
		super();
		(this as unknown as EventTarget).addEventListener('umb-login-flow', (e) => {
			if (e instanceof CustomEvent) {
				this.flow = e.detail.flow || undefined
			}
			this.requestUpdate();
		});
	}

	async firstUpdated(): Promise<void> {
		this.router = new UmbRouter(this, [
			{
				path: 'login',
				component: () => {
					const searchParams = new URLSearchParams(window.location.search);
					let flow = this.flow || searchParams.get('flow')?.toLowerCase();
					const status = searchParams.get('status');

					if (status === 'resetCodeExpired') {
						return html` <umb-error-layout
							header="Hi there"
							message=${until(umbLocalizationContext.localize('login_resetCodeExpired'), 'The link you have clicked on is invalid or has expired')}>
							<umb-back-to-login-button></umb-back-to-login-button>
						</umb-error-layout>`;
					}

					if (flow === 'invite-user' && status === 'false') {
						return html` <umb-error-layout
							header="Hi there"
							message=${until(umbLocalizationContext.localize('user_userinviteExpiredMessage'), 'Welcome to Umbraco! Unfortunately your invite has expired. Please contact your administrator and ask them to resend it.')}>
              				<umb-back-to-login-button></umb-back-to-login-button>
						</umb-error-layout>`;
					}

					// validate
					if (flow) {
						if (flow === 'mfa' && !umbAuthContext.isMfaEnabled) {
							flow = undefined;
						}
					}

					switch (flow) {
						case 'mfa':
							return html`<umb-mfa-page></umb-mfa-page>`;
						case 'reset-password':
							return html`<umb-new-password-page></umb-new-password-page>`;
						case 'invite-user':
							return html`<umb-invite-page></umb-invite-page>`;

						default:
							return html`<umb-login-page
								?allow-password-reset=${this.allowPasswordReset}
								?username-is-email=${this.usernameIsEmail}>
								<slot name="subheadline" slot="subheadline"></slot>
								<slot name="external" slot="external"></slot>
							</umb-login-page>`;
					}
				},
				default: true,
			},
			{
				path: 'login/reset',
				component: html`<umb-reset-password-page></umb-reset-password-page>`,
				action: () => (this.allowPasswordReset && !this.disableLocalLogin ? null : 'login'),
			},
		]);

		this.router.subscribe();
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.router?.unsubscribe();
	}

	render() {
		return html`
			<umb-auth-layout background-image=${ifDefined(this.backgroundImage)} logo-image=${ifDefined(this.logoImage)}>
				${this.router?.render()}
			</umb-auth-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth': UmbAuthElement;
	}
}
