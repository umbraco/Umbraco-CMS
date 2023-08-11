import type { UUIButtonState, UUIInputPasswordElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';
import UmbRouter from '../umb-router';

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

	@state()
	userId: string | null = null;

	@state()
	resetCode: string | null = null;

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const urlParams = new URLSearchParams(window.location.search);
		this.resetCode = urlParams.get('resetCode');
		this.userId = urlParams.get('userId');

		if (!this.userId || !this.resetCode) {
			// The login page should already have redirected the user to an error page. They should never get here.
			UmbRouter.redirect('login');
			return;
		}
	}

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

	#renderRoutes() {
		switch (this.page) {
			case 'new':
				return html`<umb-new-password-layout
					@submit=${this.#onSubmit}
					.userId=${this.userId!}
					.state=${this.state}
					.error=${this.error}></umb-new-password-layout>`;
			case 'done':
				return html`<umb-confirmation-layout
					header="Success!"
					message="Your password has been successfully updated"></umb-confirmation-layout>`;
		}
	}

	render() {
		return this.userId && this.resetCode ? this.#renderRoutes() : nothing;
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-new-password-page': UmbNewPasswordPageElement;
	}
}
