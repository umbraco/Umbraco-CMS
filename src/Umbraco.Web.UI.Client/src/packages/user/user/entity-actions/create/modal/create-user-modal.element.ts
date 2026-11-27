import { UmbUserDetailRepository } from '../../../repository/index.js';
import { UmbUserConfigRepository } from '../../../repository/config/index.js';
import { UmbUserKind } from '../../../utils/index.js';
import type { UmbUserKindType } from '../../../utils/index.js';
import { UMB_CREATE_USER_SUCCESS_MODAL } from './create-user-success-modal.token.js';
import type { UmbCreateUserModalData } from './create-user-modal.token.js';
import type { UmbUserGroupInputElement } from '@umbraco-cms/backoffice/user-group';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbOpenModal, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { css, html, customElement, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

@customElement('umb-create-user-modal')
export class UmbCreateUserModalElement extends UmbModalBaseElement<UmbCreateUserModalData> {
	#userDetailRepository = new UmbUserDetailRepository(this);
	#userConfigRepository = new UmbUserConfigRepository(this);

	@query('#CreateUserForm')
	private _form?: HTMLFormElement;

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

	get #showUsername() {
		return this._usernameIsEmail === false && this.data?.user.kind !== UmbUserKind.API;
	}

	async #onSubmit(e: SubmitEvent) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		const name = formData.get('name') as string;
		const email = formData.get('email') as string;

		const userGroupPicker = form.querySelector('#userGroups') as UmbUserGroupInputElement;
		const userGroupReferences: Array<UmbReferenceByUnique> = userGroupPicker?.selection.map((unique) => {
			return { unique };
		});

		const { data: userScaffold } = await this.#userDetailRepository.createScaffold();
		if (!userScaffold) return;

		userScaffold.kind = this.data?.user.kind ?? UmbUserKind.DEFAULT;
		userScaffold.name = name;
		userScaffold.email = email;
		userScaffold.userName = this.#showUsername ? (formData.get('username') as string) : email;
		userScaffold.userGroupUniques = userGroupReferences;

		const { data } = await this.#userDetailRepository.create(userScaffold);

		if (data) {
			this.#openSuccessModal(data.unique, data.kind);
		}
	}

	async #openSuccessModal(userUnique: string, userKind: UmbUserKindType) {
		await umbOpenModal(this, UMB_CREATE_USER_SUCCESS_MODAL, {
			data: {
				user: {
					unique: userUnique,
					kind: userKind,
				},
			},
		})
			.then(() => {
				this._submitModal();
			})
			.catch((reason: any) => {
				if (reason?.type === 'createAnotherUser') {
					this._form?.reset();
				} else {
					this._rejectModal();
				}
			});
	}

	override render() {
		return html`<uui-dialog-layout headline=${this.localize.term('user_createUserHeadline', this.data?.user.kind)}>
			<p>${this.localize.term('user_createUserDescription', this.data?.user.kind)}</p>

			${this.#renderForm()}
			<uui-button @click=${this._rejectModal} slot="actions" label=${this.localize.term('general_cancel')} look="secondary"></uui-button>
			<uui-button
				form="CreateUserForm"
				slot="actions"
				type="submit"
				label=${this.localize.term('user_createUser')}
				look="primary"
				color="positive"></uui-button>
		</uui-dialog-layout>`;
	}

	#renderForm() {
		return html` <uui-form>
			<form id="CreateUserForm" name="form" @submit="${this.#onSubmit}">
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
			</form>
		</uui-form>`;
	}

	#renderUsername() {
		if (!this.#showUsername) return nothing;
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
			uui-input,
			uui-input-password,
			uui-combobox {
				width: 100%;
			}

			p {
				width: 580px;
			}

			uui-textarea {
				--uui-textarea-min-height: 100px;
			}
		`,
	];
}

export default UmbCreateUserModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-user-modal': UmbCreateUserModalElement;
	}
}
