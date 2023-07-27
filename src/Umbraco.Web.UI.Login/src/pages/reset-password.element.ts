import type { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, PropertyValueMap, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';
import { when } from 'lit/directives/when.js';

import './back-to-login-button.element.js';

@customElement('umb-reset-password')
export default class UmbResetPasswordElement extends LitElement {
	@state()
	resetCallState: UUIButtonState = undefined;

	@state()
	resetCodeExpired = false;

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.resetCodeExpired = new URLSearchParams(window.location.search).get('status') === 'resetCodeExpired';
	}

	#handleResetSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		const form = e.target as HTMLFormElement;

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const username = formData.get('email') as string;

		this.resetCallState = 'waiting';
		const response = await UmbAuthMainContext.Instance.resetPassword(username);

		if (response.status === 200) {
			this.resetCallState = 'success';
		} else {
			this.resetCallState = 'failed';
		}
	};

	#resend() {
		this.resetCallState = undefined;
		this.resetCodeExpired = false;
		let urlWithoutParams = window.location.href.split('?')[0];
		window.history.replaceState({}, document.title, urlWithoutParams);
	}

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

					${when(
						this.resetCallState === 'success',
						() => html`
							<span>
								An email with password reset instructions will be sent to the specified address if it matched our
								records
							</span>
						`
					)}

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

	#renderExpiredPage() {
		return html`
			<div id="code-expired-page">
				<div id="header">
					<h2>Reset code expired</h2>
					<span>
						Password reset links are only valid for a limited time for security reasons. To reset your password, please
						request a new reset link.
					</span>
				</div>

				<uui-button
					type="submit"
					label="Request new link"
					look="primary"
					color="default"
					@click=${this.#resend}></uui-button>

				<umb-back-to-login-button></umb-back-to-login-button>
			</div>
		`;
	}

	#renderConfirmationPage() {
		return html`
			<div id="confirm-page">
				<div id="header">
					<h2>Email has been sent!</h2>
					<span>Check your inbox and click the received link to reset your password.</span>
				</div>

				<uui-button type="submit" label="Login" look="primary" color="default" href=" "></uui-button>

				<span id="resend">Didn't receive the email? <a href="reset" @click=${this.#resend} compact>Resend</a></span>
			</div>
		`;
	}

	render() {
		return this.resetCodeExpired
			? this.#renderExpiredPage()
			: this.resetCallState === 'success'
			? this.#renderConfirmationPage()
			: this.#renderResetPage();
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
			form,
			#confirm-page,
			#code-expired-page {
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
			#back-to-login {
				height: 1rem;
				margin-top: var(--uui-size-layout-2);
				color: #868686; /* TODO Change to uui color when uui gets a muted text variable */
				font-size: 14px;
				gap: var(--uui-size-space-1);
				align-self: center;
				text-decoration: none;
				display: inline-flex;
				line-height: 1;
				font-weight: 600;
			}
			a svg {
				width: 1rem;
			}
			a:hover {
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-reset-password': UmbResetPasswordElement;
	}
}
