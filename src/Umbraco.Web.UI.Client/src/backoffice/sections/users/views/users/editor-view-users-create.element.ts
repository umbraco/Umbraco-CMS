import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { UUIInputPasswordElement } from '@umbraco-ui/uui';
import { UmbModalLayoutElement } from '../../../../../core/services/modal/layouts/modal-layout.element';
import { UmbUserStore } from '../../../../../core/stores/user/user.store';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UserDetails } from '@umbraco-cms/models';
import UmbPickerUserGroupElement from '../../picker-user-group.element';
import '../../picker-user-group.element';
import { UmbNotificationService } from 'src/core/services/notification';
import { UmbNotificationDefaultData } from 'src/core/services/notification/layouts/default';

export type UsersViewType = 'list' | 'grid';
@customElement('umb-editor-view-users-create')
export class UmbEditorViewUsersCreateElement extends UmbContextConsumerMixin(UmbModalLayoutElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: center;
				height: 100%;
				width: 100%;
			}
			uui-box {
				max-width: 500px;
			}
			uui-form-layout-item {
				display: flex;
				flex-direction: column;
			}
			uui-input,
			uui-input-password {
				width: 100%;
			}
			form {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
			}
			uui-form-layout-item {
				margin-bottom: 0;
			}
			uui-textarea {
				--uui-textarea-min-height: 100px;
			}
		`,
	];

	@query('#form')
	private _form!: HTMLFormElement;

	@state()
	private _createdUser?: UserDetails;

	protected _userStore?: UmbUserStore;
	private _notificationService?: UmbNotificationService;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeAllContexts(['umbUserStore', 'umbNotificationService'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._notificationService = instances['umbNotificationService'];
		});
	}

	private _handleSubmit(e: Event) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		console.log('formData', formData);

		const name = formData.get('name') as string;
		const email = formData.get('email') as string;
		//TODO: How should we handle pickers forms?
		const userGroupPicker = form.querySelector('#userGroups') as UmbPickerUserGroupElement;
		const userGroups = userGroupPicker?.value || [];

		this._userStore?.invite(name, email, '', userGroups).then((user) => {
			if (user) {
				this._createdUser = user;
			}
		});
	}

	private _copyPassword() {
		const passwordInput = this.shadowRoot?.querySelector('#password') as UUIInputPasswordElement;
		if (!passwordInput || typeof passwordInput.value !== 'string') return;

		navigator.clipboard.writeText(passwordInput.value);
		const data: UmbNotificationDefaultData = { message: 'Password copied' };
		this._notificationService?.peek('positive', { data });
	}

	private _submitForm() {
		this._form?.requestSubmit();
	}

	private _closeModal() {
		this.modalHandler?.close();
	}

	private _resetForm() {
		this._createdUser = undefined;
	}

	private _goToProfile() {
		if (!this._createdUser) return;

		this._closeModal();
		history.pushState(null, '', '/section/users/view/users/user/' + this._createdUser?.key); //TODO: URL Should be dynamic
	}

	private _renderForm() {
		return html` <h1>Create user</h1>
			<p style="margin-top: 0">
				Create new users to give them access to Umbraco. When a user is created a password will be generated that you
				can share with the user.
			</p>
			<uui-form>
				<form id="form" name="form" @submit="${this._handleSubmit}">
					<uui-form-layout-item>
						<uui-label slot="label" for="name" required>Name</uui-label>
						<uui-input id="name" label="name" type="text" name="name" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label slot="label" for="email" required>Email</uui-label>
						<uui-input id="email" label="email" type="email" name="email" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label slot="label" for="userGroups" required>User group</uui-label>
						<span slot="description">Add groups to assign access and permissions</span>
						<umb-picker-user-group id="userGroups" name="userGroups"></umb-picker-user-group>
					</uui-form-layout-item>
				</form>
			</uui-form>`;
	}

	private _renderPostCreate() {
		if (!this._createdUser) return nothing;

		return html`<div>
			<h1><b style="color: var(--uui-color-interactive-emphasis)">${this._createdUser.name}</b> has been created</h1>
			<p>The new user has successfully been created. To log in to Umbraco use the password below</p>

			<uui-label for="password">Password</uui-label>
			<uui-input-password
				id="password"
				label="password"
				name="password"
				value="${'PUT GENERATED PASSWORD HERE'}"
				readonly>
				<!-- The button should be placed in the append part of the input, but that doesn't work with password inputs for now. -->
				<uui-button slot="prepend" compact label="copy" @click=${this._copyPassword}></uui-button>
			</uui-input-password>
		</div>`;
	}

	render() {
		return html`<uui-dialog-layout>
			${this._createdUser ? this._renderPostCreate() : this._renderForm()}
			${this._createdUser
				? html`
						<uui-button
							@click=${this._closeModal}
							style="margin-right: auto"
							slot="actions"
							label="Close"
							look="secondary"></uui-button>
						<uui-button
							@click=${this._resetForm}
							slot="actions"
							label="Create another user"
							look="secondary"></uui-button>
						<uui-button @click=${this._goToProfile} slot="actions" label="Go to profile" look="primary"></uui-button>
				  `
				: html`
						<uui-button
							@click=${this._closeModal}
							style="margin-right: auto"
							slot="actions"
							label="Cancel"
							look="secondary"></uui-button>
						<uui-button
							@click="${this._submitForm}"
							slot="actions"
							type="submit"
							label="Create user"
							look="primary"></uui-button>
				  `}
		</uui-dialog-layout>`;
	}
}

export default UmbEditorViewUsersCreateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-create': UmbEditorViewUsersCreateElement;
	}
}
