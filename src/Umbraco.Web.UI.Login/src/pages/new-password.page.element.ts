import type { UUIButtonState, UUIInputPasswordElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';

@customElement('umb-new-password-page')
export default class UmbNewPasswordPageElement extends LitElement {
	@query('#confirmPassword')
	confirmPasswordElement!: UUIInputPasswordElement;

	@state()
	state: UUIButtonState = undefined;

	@state()
	page: 'new' | 'done' | 'error' = 'new';

	async #onSubmit(event: CustomEvent) {
		event.preventDefault();
		const urlParams = new URLSearchParams(window.location.search);
		const resetCode = urlParams.get('resetCode');
		const userId = urlParams.get('userId');
		const password = event.detail.password;

		if (!resetCode || !userId) return;

		this.state = 'waiting';
		const response = await UmbAuthMainContext.Instance.newPassword(password, resetCode, userId);
		this.state = response.status === 200 ? 'success' : 'failed';
		this.page = response.status === 200 ? 'done' : 'error';
	}

	#renderConfirmationPage() {
		return html`
			<div id="confirm-page">
				<div id="header">
					<h2>Success!</h2>
					<span>Your password has been successfully updated</span>
				</div>

				<uui-button type="submit" label="Login" look="primary" color="default" href="login"></uui-button>
			</div>
		`;
	}

	#renderErrorPage() {
		return html` TODO MAKE ERROR PAGE`;
	}

	render() {
		switch (this.page) {
			case 'new':
				return html`<umb-new-password-layout @submit=${this.#onSubmit} .state=${this.state}></umb-new-password-layout>`;
			case 'done':
				return this.#renderConfirmationPage();
			case 'error':
				return this.#renderErrorPage();
		}
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
		'umb-new-password-page': UmbNewPasswordPageElement;
	}
}
