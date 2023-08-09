import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
	@state()
	state: UUIButtonState = undefined;

	@state()
	page: 'new' | 'error' = 'new';

	async #onSubmit(event: CustomEvent) {
		event.preventDefault();
		const password = event.detail.password;

		if (!password) return;

		this.state = 'waiting';
		const response = await UmbAuthMainContext.Instance.newInvitedUserPassword(password);

		if (response.status === 200) {
			window.location.href = UmbAuthMainContext.Instance.returnPath;
			this.state = 'success';
		} else {
			this.page = 'error';
			this.state = 'failed';
		}
	}

	render() {
		switch (this.page) {
			case 'new':
				return html`<umb-new-password-layout @submit=${this.#onSubmit} .state=${this.state}></umb-new-password-layout>`;
			case 'error':
				return html`ERROR PAGE`;
		}
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite-page': UmbInvitePageElement;
	}
}
