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
	page: 'new' | 'done' = 'new';

	@state()
	error = '';

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
		this.page = response.status === 200 ? 'done' : 'new';
		this.error = response.error || '';
	}

	render() {
		switch (this.page) {
			case 'new':
				return html`<umb-new-password-layout
					@submit=${this.#onSubmit}
					.state=${this.state}
					.error=${this.error}></umb-new-password-layout>`;
			case 'done':
				return html`<umb-confirmation-layout
					header="Success!"
					message="Your password has been successfully updated"></umb-confirmation-layout>`;
		}
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-password-page': UmbNewPasswordPageElement;
	}
}
