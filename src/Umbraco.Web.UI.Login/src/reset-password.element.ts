import type { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from './context/auth-main.context';

@customElement('umb-reset-password')
export default class UmbResetPasswordElement extends LitElement {
	@state()
	resetCallState: UUIButtonState = undefined;

	#handleResetSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		const form = e.target as HTMLFormElement;

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const username = formData.get('email') as string;

		UmbAuthMainContext.Instance.resetPassword(username);
	};

	#renderResetPage() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
					Enter the email address associated with your account and we'll send you a link to reset your password.
					<uui-form-layout-item>
						<uui-label id="emailLabel" for="email" slot="label" required>Email</uui-label>
						<uui-input
							type="email"
							id="email"
							name="email"
							label="Email"
							required
							required-message="Email is required"></uui-input>
					</uui-form-layout-item>

					<uui-button
						type="submit"
						label="Continue"
						look="primary"
						color="positive"
						.state=${this.resetCallState}></uui-button>
				</form>
			</uui-form>
		`;
	}

	#renderConfirmationPage() {
		return html`
			Information about the reset has been sent to your email address. Please follow the instructions in the email to
			reset your password.
		`;
	}

	render() {
		return this.resetCallState === 'success' ? this.#renderConfirmationPage() : this.#renderResetPage();
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-input {
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
		'umb-reset-password': UmbResetPasswordElement;
	}
}
