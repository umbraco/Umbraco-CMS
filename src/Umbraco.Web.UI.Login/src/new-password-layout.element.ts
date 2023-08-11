import type { UUIButtonState, UUIInputPasswordElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from './context/auth-main.context';

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
	userId = '';

	@state()
	passwordConfig?: {
		allowManuallyChangingPassword: boolean;
		minNonAlphaNumericChars: number;
		minPasswordLength: number;
	};

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const response = await UmbAuthMainContext.Instance.getPasswordConfig(this.userId);
		this.passwordConfig = response.data;
	}

	#onSubmit(event: Event) {
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

		if (this.passwordConfig.minPasswordLength > 0) {
			if (password.length < this.passwordConfig?.minPasswordLength) {
				this.passwordElement.setCustomValidity(
					`Password must be at least ${this.passwordConfig?.minPasswordLength} characters long`
				);
				return;
			}
		}

		if (this.passwordConfig.minNonAlphaNumericChars > 0) {
			const nonAlphaNumericChars = password.replace(/[a-zA-Z0-9]/g, '').length; //TODO: How should we check for non-alphanumeric chars?
			if (nonAlphaNumericChars < this.passwordConfig?.minNonAlphaNumericChars) {
				this.passwordElement.setCustomValidity(
					`Password must contain at least ${this.passwordConfig?.minNonAlphaNumericChars} non-alphanumeric characters`
				);
				return;
			}
		}

		if (password !== passwordConfirm) {
			this.confirmPasswordElement.setCustomValidity('Passwords do not match');
			return;
		}

		this.dispatchEvent(new CustomEvent('submit', { detail: { password } }));
	}

	render() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit=${this.#onSubmit}>
					<div id="header">
						<h2>Create new password</h2>
						<span> Enter a new password for your account. </span>
					</div>
					<uui-form-layout-item>
						<uui-label id="passwordLabel" for="password" slot="label" required>Password</uui-label>
						<uui-input-password
							type="password"
							id="password"
							name="password"
							label="Password"
							required
							required-message="Password is required"></uui-input-password>
					</uui-form-layout-item>

					<uui-form-layout-item>
						<uui-label id="confirmPasswordLabel" for="confirmPassword" slot="label" required>
							Confirm password
						</uui-label>
						<uui-input-password
							type="password"
							id="confirmPassword"
							name="confirmPassword"
							label="ConfirmPassword"></uui-input-password>
					</uui-form-layout-item>

					${this.#renderErrorMessage()}

					<uui-button type="submit" label="Continue" look="primary" color="default" .state=${this.state}></uui-button>
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
		UUITextStyles,
		css`
			#header {
				text-align: center;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}
			#header span {
				color: #868686; /* TODO Change to uui color when uui gets a muted text variable */
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
