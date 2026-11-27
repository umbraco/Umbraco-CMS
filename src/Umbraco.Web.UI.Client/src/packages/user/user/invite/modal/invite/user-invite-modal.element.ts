import type { UmbUserGroupInputElement } from '../../../../user-group/components/input-user-group/user-group-input.element.js';
import { UmbInviteUserRepository } from '../../repository/invite-user.repository.js';
import { UmbUserConfigRepository } from '../../../repository/config/index.js';
import { css, html, customElement, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

@customElement('umb-invite-user-modal')
export class UmbInviteUserModalElement extends UmbModalBaseElement {
	#inviteUserRepository = new UmbInviteUserRepository(this);
	#userConfigRepository = new UmbUserConfigRepository(this);

	@state()
	private _usernameIsEmail = true;

	constructor() {
		super();
		this.observe(
			this.#userConfigRepository.part('usernameIsEmail'),
			(usernameIsEmail) => (this._usernameIsEmail = usernameIsEmail ?? true),
			null,
		);
	}

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

		const { error } = await this.#inviteUserRepository.invite({
			name,
			email,
			userName: this._usernameIsEmail ? email : (formData.get('username') as string),
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
					${this.#renderUsername()}
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

	#renderUsername() {
		if (this._usernameIsEmail) return nothing;
		return html`<uui-form-layout-item>
			<uui-label id="usernameLabel" slot="label" for="username" required>
				<umb-localize key="user_loginname"></umb-localize>
			</uui-label>
			<uui-input
				id="username"
				label=${this.localize.term('user_loginname')}
				type="text"
				name="username"
				autocomplete="off"
				required
				required-message=${this.localize.term('user_loginnameRequired')}></uui-input>
		</uui-form-layout-item>`;
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
