import type { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';
import { when } from 'lit/directives/when.js';

@customElement('umb-reset-password-page')
export default class UmbResetPasswordPageElement extends LitElement {
	@state()
	resetCallState: UUIButtonState = undefined;

	@state()
	error = '';

	#handleResetSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		const form = e.target as HTMLFormElement;

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const username = formData.get('email') as string;

		this.resetCallState = 'waiting';
		const response = await UmbAuthMainContext.Instance.resetPassword(username);
		this.resetCallState = response.status === 200 ? 'success' : 'failed';
		this.error = response.error || '';
	};

	#renderResetPage() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
					<div id="header">
						<h2>Forgot your password?</h2>
						<span>
							Enter the email address associated with your account and we'll send you the reset instructions.
						</span>
					</div>

					<uui-form-layout-item>
						<uui-label for="email" slot="label" required>Email</uui-label>
						<uui-input
							type="email"
							id="email"
							name="email"
							label="Email"
							required
							required-message="Email is required"></uui-input>
					</uui-form-layout-item>

					${this.#renderErrorMessage()}

					<uui-button
						type="submit"
						label="Reset password"
						look="primary"
						color="default"
						.state=${this.resetCallState}></uui-button>
				</form>
			</uui-form>

			<umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
		`;
	}

	#renderErrorMessage() {
		if (!this.error || this.resetCallState !== 'failed') return nothing;

		return html`<span class="text-danger">${this.error}</span>`;
	}

	#renderConfirmationPage() {
		return html`
			<umb-confirmation-layout
				header="Email has been sent!"
				message="Check your inbox and click the received link to reset your password.">
				<span id="resend">
					Didn't receive the email?
					<a @click=${() => (this.resetCallState = undefined)} href="login/reset" compact>Resend</a>
				</span>
			</umb-confirmation-layout>
		`;
	}

	render() {
		return this.resetCallState === 'success' ? this.#renderConfirmationPage() : this.#renderResetPage();
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
				gap: var(--uui-size-layout-2);
			}
			uui-form-layout-item {
				margin: 0;
			}
			uui-input,
			uui-input-password {
				width: 100%;
				border-radius: var(--uui-border-radius);
			}
			uui-input {
				width: 100%;
			}
			uui-button {
				width: 100%;
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}

			#resend {
				display: inline-flex;
				font-size: 14px;
				align-self: center;
				gap: var(--uui-size-space-1);
			}

			#resend a {
				color: var(--uui-color-selected);
				font-weight: 600;
				text-decoration: none;
			}
			#resend a:hover {
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-reset-password-page': UmbResetPasswordPageElement;
	}
}
