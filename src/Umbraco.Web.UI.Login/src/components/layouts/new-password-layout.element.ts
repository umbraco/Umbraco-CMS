import type { UUIButtonState, UUIInputPasswordElement } from '@umbraco-ui/uui';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';

import { umbAuthContext } from '../../context/auth.context.js';
import { umbLocalizationContext } from '../../external/localization/localization-context.js';

@customElement('umb-new-password-layout')
export default class UmbNewPasswordLayoutElement extends LitElement {
	@query('#password')
	passwordElement!: UUIInputPasswordElement;

	@query('#confirmPassword')
	confirmPasswordElement!: UUIInputPasswordElement;

	@property()
	state: UUIButtonState = undefined;

	@property()
	error: string = '';

	@property()
	user: any;

	@state()
	passwordConfig?: {
		allowManuallyChangingPassword: boolean;
		minNonAlphaNumericChars: number;
		minPasswordLength: number;
	};

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const response = await umbAuthContext.getPasswordConfig(this.user.id);
		this.passwordConfig = response.data;
	}

	async #onSubmit(event: Event) {
		event.preventDefault();
		if (!this.passwordConfig) return;
		const form = event.target as HTMLFormElement;

		this.passwordElement.setCustomValidity('');
		this.confirmPasswordElement.setCustomValidity('');

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const password = formData.get('password') as string;
		const passwordConfirm = formData.get('confirmPassword') as string;

		let passwordIsInvalid = false;

		if (this.passwordConfig.minPasswordLength > 0 && password.length < this.passwordConfig.minPasswordLength) {
			passwordIsInvalid = true;
		}

		if (this.passwordConfig.minNonAlphaNumericChars > 0) {
			const nonAlphaNumericChars = password.replace(/[a-zA-Z0-9]/g, '').length; //TODO: How should we check for non-alphanumeric chars?
			if (nonAlphaNumericChars < this.passwordConfig?.minNonAlphaNumericChars) {
				passwordIsInvalid = true;
			}
		}

		if (passwordIsInvalid) {
			const passwordValidityText = await umbLocalizationContext.localize('errorHandling_errorInPasswordFormat', [this.passwordConfig.minPasswordLength, this.passwordConfig.minNonAlphaNumericChars], "The password doesn't meet the minimum requirements!");
			this.passwordElement.setCustomValidity(passwordValidityText);
			return;
		}

		if (password !== passwordConfirm) {
			const passwordValidityText = await umbLocalizationContext.localize('user_passwordMismatch', undefined, "The confirmed password doesn't match the new password!");
			this.confirmPasswordElement.setCustomValidity(passwordValidityText);
			return;
		}

		this.dispatchEvent(new CustomEvent('submit', { detail: { password } }));
	}

	render() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit=${this.#onSubmit}>
					<div id="header">
						<h2>Hi, ${this.user.name}</h2>
            			<umb-localize key="user_userinviteWelcomeMessage">Welcome to Umbraco! Just need to get your password setup and then you're good to go</umb-localize>
					</div>
					<uui-form-layout-item>
						<uui-label id="passwordLabel" for="password" slot="label" required>
						  <umb-localize key="user_newPassword">New password</umb-localize>
						</uui-label>
						<uui-input-password
							type="password"
							id="password"
							name="password"
							label=${until(umbLocalizationContext.localize('user_newPassword'), 'New password')}
							required
							required-message=${until(umbLocalizationContext.localize('user_passwordIsBlank', undefined, 'Your new password cannot be blank!'))}></uui-input-password>
					</uui-form-layout-item>

					<uui-form-layout-item>
						<uui-label id="confirmPasswordLabel" for="confirmPassword" slot="label" required>
              				<umb-localize key="user_confirmNewPassword">Confirm new password</umb-localize>
						</uui-label>
						<uui-input-password
							type="password"
							id="confirmPassword"
							name="confirmPassword"
							label=${until(umbLocalizationContext.localize('user_confirmNewPassword'), 'Confirm new password')}
              				required
              				required-message=${until(umbLocalizationContext.localize('general_required', undefined, 'Required'))}></uui-input-password>
					</uui-form-layout-item>

					${this.#renderErrorMessage()}

					<uui-button type="submit" label=${until(umbLocalizationContext.localize('general_continue'), 'Continue')} look="primary" color="default" .state=${this.state}></uui-button>
				</form>
			</uui-form>

			<umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
		`;
	}

	#renderErrorMessage() {
		if (!this.error || this.state !== 'failed') return nothing;

		return html`<span class="text-danger">${this.error}</span>`;
	}

	static styles: CSSResultGroup = [
		css`
			#header {
				text-align: center;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}
			#header span {
				color: var(--uui-color-text-alt); /* TODO Change to uui color when uui gets a muted text variable */
				font-size: 14px;
			}
			#header h2 {
				margin: 0px;
				font-weight: bold;
				font-size: 1.4rem;
			}
			form {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}
			uui-form-layout-item {
				margin: 0;
			}
			h2 {
				margin: 0px;
				font-weight: 600;
				font-size: 1.4rem;
				margin-bottom: var(--uui-size-space-4);
			}
			uui-input-password {
				width: 100%;
			}
			uui-button {
				width: 100%;
				margin-top: var(--uui-size-space-5);
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}
			.text-danger {
				color: var(--uui-color-danger-standalone);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-password-layout': UmbNewPasswordLayoutElement;
	}
}
