import { UmbUserDetailRepository } from '../../../repository/index.js';
import { UmbUserKind } from '../../../utils/index.js';
import { UMB_CREATE_USER_SUCCESS_MODAL } from './create-user-success-modal.token.js';
import type { UmbCreateUserModalData } from './create-user-modal.token.js';
import type { UmbUserGroupInputElement } from '@umbraco-cms/backoffice/user-group';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { css, html, customElement, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

@customElement('umb-create-user-modal')
export class UmbCreateUserModalElement extends UmbModalBaseElement<UmbCreateUserModalData> {
	#userDetailRepository = new UmbUserDetailRepository(this);

	@query('#CreateUserForm')
	_form?: HTMLFormElement;

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
		userScaffold.userName = email;
		userScaffold.userGroupUniques = userGroupReferences;

		// TODO: figure out when to use email or username
		const { data } = await this.#userDetailRepository.create(userScaffold);

		if (data) {
			if (data.kind === UmbUserKind.DEFAULT) {
				this.#openSuccessModal(data.unique);
			} else {
				this._submitModal();
			}
		}
	}

	async #openSuccessModal(userUnique: string) {
		await umbOpenModal(this, UMB_CREATE_USER_SUCCESS_MODAL, {
			data: {
				user: {
					unique: userUnique,
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
			<uui-button @click=${this._rejectModal} slot="actions" label="Cancel" look="secondary"></uui-button>
			<uui-button
				form="CreateUserForm"
				slot="actions"
				type="submit"
				label="Create user"
				look="primary"
				color="positive"></uui-button>
		</uui-dialog-layout>`;
	}

	#renderForm() {
		return html` <uui-form>
			<form id="CreateUserForm" name="form" @submit="${this.#onSubmit}">
				<uui-form-layout-item>
					<uui-label id="nameLabel" slot="label" for="name" required>Name</uui-label>
					<uui-input id="name" label="name" type="text" name="name" required></uui-input>
				</uui-form-layout-item>
				<uui-form-layout-item>
					<uui-label id="emailLabel" slot="label" for="email" required>Email</uui-label>
					<uui-input id="email" label="email" type="email" name="email" required></uui-input>
				</uui-form-layout-item>
				<uui-form-layout-item>
					<uui-label id="userGroupsLabel" slot="label" for="userGroups" required>User group</uui-label>
					<span slot="description">Add groups to assign access and permissions</span>
					<umb-user-group-input id="userGroups" name="userGroups"></umb-user-group-input>
				</uui-form-layout-item>
			</form>
		</uui-form>`;
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
