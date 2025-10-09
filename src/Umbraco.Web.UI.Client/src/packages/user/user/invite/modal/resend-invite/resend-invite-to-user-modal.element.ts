import { UmbInviteUserRepository } from '../../repository/invite-user.repository.js';
import type {
	UmbResendInviteToUserModalData,
	UmbResendInviteToUserModalValue,
} from './resend-invite-to-user-modal.token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-resend-invite-to-user-modal')
export class UmbResendInviteToUserModalElement extends UmbModalBaseElement<
	UmbResendInviteToUserModalData,
	UmbResendInviteToUserModalValue
> {
	#userInviteUserRepository = new UmbInviteUserRepository(this);

	async #onSubmitForm(e: Event) {
		e.preventDefault();

		if (!this.modalContext?.data.user.unique) throw new Error('User unique is missing');

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const message = formData.get('message') as string;

		const { error } = await this.#userInviteUserRepository.resendInvite({
			user: {
				unique: this.modalContext.data.user.unique,
			},
			message,
		});

		if (!error) {
			this._submitModal();
		}
	}

	override render() {
		return html`<uui-dialog-layout headline="Resend invite">
			${this.#renderForm()}

			<uui-button @click=${this._rejectModal} slot="actions" label="Cancel" look="secondary"></uui-button>
			<uui-button
				slot="actions"
				type="submit"
				label="Resend invite"
				look="primary"
				color="positive"
				form="ResendInviteToUserForm"></uui-button>
		</uui-dialog-layout>`;
	}

	#renderForm() {
		return html` <uui-form>
			<form id="ResendInviteToUserForm" name="form" @submit="${this.#onSubmitForm}">
				<uui-form-layout-item>
					<uui-label id="messageLabel" slot="label" for="message" required>Message</uui-label>
					<uui-textarea id="message" label="message" name="message" required></uui-textarea>
				</uui-form-layout-item>
			</form>
		</uui-form>`;
	}
}

export default UmbResendInviteToUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-resend-invite-to-user-modal': UmbResendInviteToUserModalElement;
	}
}
