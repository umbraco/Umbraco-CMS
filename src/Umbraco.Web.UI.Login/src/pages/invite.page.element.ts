import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbAuthMainContext } from '../context/auth-main.context';
import UmbRouter from '../umb-router';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
	@state()
	state: UUIButtonState = undefined;

	@state()
	error = '';

	@state()
	userId: any = undefined;

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const response = await UmbAuthMainContext.Instance.getInvitedUser();
		this.userId = response.user?.id;

		if (!this.userId) {
			// The login page should already have redirected the user to an error page. They should never get here.
			UmbRouter.redirect('login');
			return;
		}
	}

	async #onSubmit(event: CustomEvent) {
		event.preventDefault();
		const password = event.detail.password;

		if (!password) return;

		this.state = 'waiting';
		const response = await UmbAuthMainContext.Instance.newInvitedUserPassword(password);

		if (response.error) {
			this.error = response.error;
			this.state = 'failed';
			return;
		}

		this.state = 'success';
		window.location.href = UmbAuthMainContext.Instance.returnPath;
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
