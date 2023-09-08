import type { UUIButtonState } from '@umbraco-ui/uui';
import { LitElement, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { umbAuthContext } from '../../context/auth.context.js';
import UmbRouter from '../../utils/umb-router.js';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends LitElement {
	@state()
	state: UUIButtonState = undefined;

	@state()
	error = '';

	@state()
	userId?: string;

	protected async firstUpdated(_changedProperties: any) {
		super.firstUpdated(_changedProperties);

		const response = await umbAuthContext.getInvitedUser();
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
		const response = await umbAuthContext.newInvitedUserPassword(password);

		if (response.error) {
			this.error = response.error;
			this.state = 'failed';
			return;
		}

		this.state = 'success';
		window.location.href = umbAuthContext.returnPath;
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite-page': UmbInvitePageElement;
	}
}
