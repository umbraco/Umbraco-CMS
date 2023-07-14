import { UUIButtonState, UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';

@customElement('umb-reset')
export default class UmbResetElement extends LitElement {
	@state()
	resetCallState: UUIButtonState = undefined;

	#handleResetSubmit = async (e: SubmitEvent) => {
		e.preventDefault();
		const form = e.target as HTMLFormElement;

		if (!form) return;
		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const username = formData.get('email') as string;

		//TODO call api

		this.dispatchEvent(new CustomEvent('login-success', { bubbles: true, composed: true }));
	};

	#renderResetPage() {
		return html`
			<uui-form>
				<form id="LoginForm" name="login" @submit="${this.#handleResetSubmit}">
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
						label="Login"
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
		return this.resetCallState === 'success' ? this.#renderResetPage() : this.#renderConfirmationPage();
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-reset': UmbResetElement;
	}
}
