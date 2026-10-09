import type { UmbUserGroupInputElement } from '../../../../user-group/components/input-user-group/user-group-input.element.js';
import { UmbInviteUserRepository } from '../../repository/invite-user.repository.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

@customElement('umb-invite-user-modal')
export class UmbInviteUserModalElement extends UmbModalBaseElement {
	#inviteUserRepository = new UmbInviteUserRepository(this);

	async #onSubmit(e: Event) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		const name = formData.get('name') as string;
		const email = formData.get('email') as string;

		//TODO: How should we handle pickers forms?
		const userGroupPicker = form.querySelector('#userGroups') as UmbUserGroupInputElement;
		const userGroupUniques: Array<UmbReferenceByUnique> = userGroupPicker?.selection.map((unique) => {
			return { unique };
		});

		const message = formData.get('message') as string;

		// TODO: figure out when to use email or username
		const { error } = await this.#inviteUserRepository.invite({
			name,
			email,
			userName: email,
			message,
			userGroupUniques,
		});

		if (!error) {
			this._submitModal();
		}
	}

	override render() {
		return html`<uui-dialog-layout headline=${this.localize.term('user_inviteUser')}>
			${this.#renderForm()}
			<uui-button @click=${this._rejectModal} slot="actions" label=${this.localize.term('general_cancel')} look="secondary"></uui-button>
			<uui-button
				form="InviteUserForm"
				slot="actions"
				type="submit"
				label=${this.localize.term('user_sendInvite')}
				look="primary"
				color="positive"></uui-button
		></uui-dialog-layout>`;
	}

	#renderForm() {
		return html` <p style="margin-top: 0">
				<umb-localize key="user_inviteUserHelp"></umb-localize>
			</p>
			<uui-form>
				<form id="InviteUserForm" name="form" @submit="${this.#onSubmit}">
					<uui-form-layout-item>
						<uui-label id="nameLabel" slot="label" for="name" required>
							<umb-localize key="general_name"></umb-localize>
						</uui-label>
						<uui-input id="name" label="name" type="text" name="name" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label id="emailLabel" slot="label" for="email" required>
							<umb-localize key="general_email"></umb-localize>
						</uui-label>
						<uui-input id="email" label="email" type="email" name="email" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label id="userGroupsLabel" slot="label" for="userGroups" required>
							<umb-localize key="user_usergroups"></umb-localize>
						</uui-label>
						<span slot="description">
							<umb-localize key="user_groupsHelp"></umb-localize>
						</span>
						<umb-user-group-input id="userGroups" name="userGroups"></umb-user-group-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label id="messageLabel" slot="label" for="message" required>
							<umb-localize key="general_message"></umb-localize>
						</uui-label>
						<uui-textarea id="message" label="message" name="message" required></uui-textarea>
					</uui-form-layout-item>
				</form>
			</uui-form>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: center;
				height: 100%;
				width: 100%;
			}

			uui-input {
				width: 100%;
			}

			uui-textarea {
				--uui-textarea-min-height: 100px;
			}
		`,
	];
}

export default UmbInviteUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-invite-user-modal': UmbInviteUserModalElement;
	}
}
