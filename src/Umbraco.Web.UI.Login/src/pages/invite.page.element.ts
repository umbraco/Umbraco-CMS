import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
	@state()
	state: UUIButtonState = undefined;

	@state()
	error = '';

	@state()
	userId: any = undefined;

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
			return;
		}

		this.state = 'failed';
		alert('TODO: SHOW ERROR');
	}

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const { user } = await UmbAuthMainContext.Instance.getInvitedUser();
		this.userId = user.id;

		if (!this.userId) {
			alert('TODO: SHOW ERROR');
			return;
		}
	}

	render() {
		return this.userId
			? html`<umb-new-password-layout
					@submit=${this.#onSubmit}
					.userId=${this.userId}
					.state=${this.state}
					.error=${this.error}></umb-new-password-layout>`
			: nothing;
	}

	static styles: CSSResultGroup = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite-page': UmbInvitePageElement;
	}
}
