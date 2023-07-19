import type { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from './context/auth-main.context';

@customElement('umb-new-password')
export default class UmbNewPasswordElement extends LitElement {
	@state()
	code: string = '';

	@state()
	newCallState: UUIButtonState = undefined;

	@state()
	private page: (typeof this.pages)[number] = 'validate';
	pages = ['validate', 'new', 'done', 'error'] as const;

	connectedCallback(): void {
		super.connectedCallback();

		this.#validateCode();
	}

	#validateCode = async () => {
		// get url params
		const urlParams = new URLSearchParams(window.location.search);
		this.code = urlParams.get('code') || '';

		const response = await UmbAuthMainContext.Instance.validatePasswordResetCode(this.code);

		this.page = response.status === 200 ? 'new' : 'error';
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

		this.newCallState = 'waiting';
		const response = await UmbAuthMainContext.Instance.newPassword(this.code, password);

		if (response.status === 200) {
			this.newCallState = 'success';
			this.page = 'done';
		} else {
			this.newCallState = 'failed';
			this.page = 'error';
		}
	};

	#renderNewPasswordPage() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
					<h2>Create new password</h2>
					Enter a new password for your account.
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
						color="positive"
						.state=${this.newCallState}></uui-button>
				</form>
			</uui-form>
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
		switch (this.page) {
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
			uui-input-password {
				width: 100%;
			}
			uui-button {
				margin-left: auto;
				display: flex;
				width: fit-content;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-password': UmbNewPasswordElement;
	}
}
