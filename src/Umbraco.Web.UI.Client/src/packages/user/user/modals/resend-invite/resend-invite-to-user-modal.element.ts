import { UmbInviteUserRepository } from '../../repository/invite/invite-user.repository.js';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-resend-invite-to-user-modal')
export class UmbResendInviteToUserModalElement extends UmbModalBaseElement {
	@query('#form')
	private _form!: HTMLFormElement;

	#userRepository = new UmbInviteUserRepository(this);

	async #onSubmitForm(e: Event) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const message = formData.get('message') as string;

		const { error } = await this.#userRepository.resendInvite({
			message,
		});

		debugger;
	}

	private _closeModal() {
		this.modalContext?.reject();
	}

	render() {
		return html`<uui-dialog-layout headline="Resend invite">
			${this.#renderForm()}

			<uui-button @click=${this._closeModal} slot="actions" label="Cancel" look="secondary"></uui-button>
			<uui-button
				slot="actions"
				type="submit"
				label="Resend invite"
				look="primary"
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

	static styles = [UmbTextStyles, css``];
}

export default UmbResendInviteToUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-resend-invite-to-user-modal': UmbResendInviteToUserModalElement;
	}
}
