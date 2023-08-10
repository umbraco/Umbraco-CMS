import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, PropertyValueMap, css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
	@state()
	state: UUIButtonState = undefined;

	@state()
	error = '';

	@state()
	user: any = undefined;

	async #onSubmit(event: CustomEvent) {
		event.preventDefault();
		const password = event.detail.password;

		if (!password) return;

		this.state = 'waiting';
		const response = await UmbAuthMainContext.Instance.newInvitedUserPassword(password);
		this.error = response.error || '';

		if (response.status === 200) {
			window.location.href = UmbAuthMainContext.Instance.returnPath;
			this.state = 'success';
		} else {
			this.state = 'failed';
		}
	}

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const user = await UmbAuthMainContext.Instance.getInvitedUser();

		if (!user) {
			alert('TODO: SHOW ERROR');
			return;
		}

		this.user = user;
		console.log(user, 'user');
	}

	render() {
		return html`<umb-new-password-layout
			@submit=${this.#onSubmit}
			.state=${this.state}
			.error=${this.error}></umb-new-password-layout>`;
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite-page': UmbInvitePageElement;
	}
}
