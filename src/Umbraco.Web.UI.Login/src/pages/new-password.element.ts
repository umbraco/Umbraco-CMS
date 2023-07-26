import type { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';

@customElement('umb-new-password')
export default class UmbNewPasswordElement extends LitElement {
	@state()
	resetCode?: string;

	@state()
	userId?: string;

	@state()
	newCallState: UUIButtonState = undefined;

	@state()
	private state: (typeof this.states)[number] = 'validate';
	states = ['validate', 'new', 'done', 'error'] as const;

	connectedCallback(): void {
		super.connectedCallback();

		this.#validateCode();
	}

	#validateCode = async () => {
		// get url params
		const urlParams = new URLSearchParams(window.location.search);
		this.resetCode = urlParams.get('resetCode') || '';
		this.userId = urlParams.get('userId') || '';

		//TODO: TEMP FIX
		this.resetCode = this.resetCode.replace(/ /g, '+');

		if (this.resetCode && this.userId) {
			this.state = 'new';
		} else {
			this.state = 'error';
		}
	};

	#handleResetSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		const form = e.target as HTMLFormElement;

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const password = formData.get('password') as string;
		const passwordConfirm = formData.get('confirmPassword') as string;

		if (password !== passwordConfirm) {
			//TODO: tell user that the passwords are not the same
			return;
		}

		if (!this.resetCode || !this.userId) return;

		this.newCallState = 'waiting';
		const response = await UmbAuthMainContext.Instance.newPassword(password, this.resetCode, this.userId);

		if (response.status === 200) {
			this.newCallState = 'success';
			this.state = 'done';
		} else {
			this.newCallState = 'failed';
			this.state = 'error';
		}
	};

	#renderNewPasswordPage() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
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
							label="ConfirmPassword"
							required
							required-message="ConfirmPassword is required"></uui-input-password>
					</uui-form-layout-item>

					<uui-button
						type="submit"
						label="Continue"
						look="primary"
						color="default"
						.state=${this.newCallState}></uui-button>
				</form>
			</uui-form>

			<umb-back-to-login-button style="margin-top: var(--uui-size-space-6)"></umb-back-to-login-button>
		`;
	}

	#renderValidatePage() {
		return html`
			Checking reset code...
			<uui-loader-circle></uui-loader-circle>
		`;
	}

	#renderConfirmationPage() {
		return html` PASSWORD SUCCESSFULLY CHANGED PAGE `;
	}

	#renderErrorPage() {
		return html` ERROR PAGE use param ?code=valid to test`;
	}

	#renderRoute() {
		switch (this.state) {
			case 'validate':
				return this.#renderValidatePage();
			case 'new':
				return this.#renderNewPasswordPage();
			case 'done':
				return this.#renderConfirmationPage();
			case 'error':
				return this.#renderErrorPage();
		}
	}

	render() {
		return this.#renderRoute();
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-password': UmbNewPasswordElement;
	}
}
