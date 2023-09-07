import type { UUIButtonState } from '@umbraco-ui/uui';
import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { when } from 'lit/directives/when.js';
import { until } from 'lit/directives/until.js';

import { UmbAuthMainContext } from '../../context/auth-main.context.js';
import UmbRouter from '../../utils/umb-router.js';
import { umbLocalizationContext } from '../../external/localization/localization-context.ts';

@customElement('umb-login-page')
export default class UmbLoginPageElement extends LitElement {
	#authContext = UmbAuthMainContext.Instance;

	@property({ type: Boolean, attribute: 'username-is-email' })
	usernameIsEmail = false;

	@property({ type: Boolean, attribute: 'allow-password-reset' })
	allowPasswordReset = false;

	@state()
	private _loginState: UUIButtonState = undefined;

	@state()
	private _loginError = '';

	@state()
	private get disableLocalLogin() {
		return UmbAuthMainContext.Instance.disableLocalLogin;
	}

	#handleSubmit = async (e: SubmitEvent) => {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);

		const username = formData.get('email') as string;
		const password = formData.get('password') as string;
		const persist = formData.has('persist');

		this._loginState = 'waiting';

		const response = await this.#authContext.login({
			username,
			password,
			persist,
		});

		this._loginError = response.error || '';
		this._loginState = response.error ? 'failed' : 'success';

		// Check for 402 status code indicating that MFA is required
		if (response.status === 402) {
			UmbAuthMainContext.Instance.isMfaEnabled = true;
			if (response.twoFactorView) {
				UmbAuthMainContext.Instance.twoFactorView = response.twoFactorView;
			}
			UmbRouter.redirect('login?flow=mfa');
			return;
		}

		if (response.error) return;

		const returnPath = this.#authContext.returnPath;

		if (returnPath) {
			location.href = returnPath;
		}

		this.dispatchEvent(new CustomEvent('login-success', { bubbles: true, composed: true }));
	};

	get #greetingLocalizationKey() {
		return [
			'login_greeting0',
			'login_greeting1',
			'login_greeting2',
			'login_greeting3',
			'login_greeting4',
			'login_greeting5',
			'login_greeting6',
		][new Date().getDay()];
	}

	render() {
		return html`
			<h1 id="greeting" class="uui-h3">
				<umb-localize .key=${this.#greetingLocalizationKey}>Welcome to Umbraco</umb-localize>
			</h1>
			${this.disableLocalLogin
				? nothing
				: html`
						<uui-form>
							<form id="LoginForm" name="login" @submit="${this.#handleSubmit}">
								<uui-form-layout-item>
									<uui-label id="emailLabel" for="email" slot="label" required>
										${this.usernameIsEmail
											? html`<umb-localize key="general_email">Email</umb-localize>`
											: html`<umb-localize key="user_username">Name</umb-localize>`}
									</uui-label>
									<uui-input
										type=${this.usernameIsEmail ? 'email' : 'text'}
										id="email"
										name="email"
										label=${this.usernameIsEmail
											? until(umbLocalizationContext.localize('general_email', 'Email'))
											: until(umbLocalizationContext.localize('user_username', 'Username'))}
										required
										required-message="Email is required"></uui-input>
								</uui-form-layout-item>

								<uui-form-layout-item>
									<uui-label id="passwordLabel" for="password" slot="label" required>
										<umb-localize key="user_password">Password</umb-localize>
									</uui-label>
									<uui-input-password
										id="password"
										name="password"
										label=${until(umbLocalizationContext.localize('user_password'), 'Password')}
										required
										required-message="Password is required"></uui-input-password>
								</uui-form-layout-item>

								<div id="secondary-actions">
									${when(
										this.#authContext.supportsPersistLogin,
										() => html`<uui-form-layout-item>
											<uui-checkbox name="persist" label="Remember me">
												<umb-localize key="user_rememberMe">Remember me</umb-localize>
											</uui-checkbox>
										</uui-form-layout-item>`
									)}
									${when(
										this.allowPasswordReset,
										() =>
											html`<a id="forgot-password" href="login/reset">
												<umb-localize key="user_forgotPassword">Forgot password?</umb-localize>
											</a>`
									)}
								</div>

								${this.#renderErrorMessage()}

								<uui-button
									type="submit"
									id="login-button"
									look="primary"
									label="Login"
									color="default"
									.state=${this._loginState}>
									<umb-localize key="general_login">Login</umb-localize>
								</uui-button>
							</form>
						</uui-form>
				  `}
			<umb-external-login-providers-layout .showDivider=${!this.disableLocalLogin}>
				<slot name="external"></slot>
			</umb-external-login-providers-layout>
		`;
	}

	#renderErrorMessage() {
		if (!this._loginError || this._loginState !== 'failed') return nothing;

		return html`<span class="text-danger">${this._loginError}</span>`;
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
			#greeting {
				text-align: center;
				margin: 0px;
				font-weight: 600;
				font-size: 1.4rem;
				margin-bottom: var(--uui-size-space-6);
			}
			form {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}
			uui-form-layout-item {
				margin: 0;
			}
			uui-input,
			uui-input-password {
				width: 100%;
				border-radius: var(--uui-border-radius);
			}
			#login-button {
				width: 100%;
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}
			#forgot-password {
				color: var(--uui-color-interactive);
				text-decoration: none;
			}
			#forgot-password:hover {
				color: var(--uui-color-interactive-emphasis);
			}
			.text-danger {
				color: var(--uui-color-danger-standalone);
			}
			#secondary-actions {
				display: flex;
				align-items: center;
				justify-content: space-between;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-login-page': UmbLoginPageElement;
	}
}
