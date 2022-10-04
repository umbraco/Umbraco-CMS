import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import { Subscription } from 'rxjs';
import './list-view-layouts/table/editor-view-users-table.element';
import './editor-view-users-grid.element';
import './editor-view-users-selection.element';
import { IRoute } from 'router-slot';
import UmbEditorViewUsersElement, { UserItem } from './editor-view-users.element';
import { UUIPopoverElement } from '@umbraco-ui/uui';

export type UsersViewType = 'list' | 'grid';
@customElement('umb-editor-view-users-invite')
export class UmbEditorViewUsersInviteElement extends UmbContextConsumerMixin(LitElement) {
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
			uui-input {
				width: 100%;
			}
			#post-invite-buttons {
				display: flex;
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
			uui
		`,
	];

	@state()
	private _showPostInvite = false;

	private _usersContext?: UmbEditorViewUsersElement;

	private _invitedUser?: UserItem;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUsersContext', (usersContext: UmbEditorViewUsersElement) => {
			this._usersContext = usersContext;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
	}

	private _handleSubmit(e: Event) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		const name = formData.get('name') as string;
		const email = formData.get('email') as string;
		const userGroup = formData.get('userGroup') as string;
		const message = formData.get('message') as string;

		this._invitedUser = this._usersContext?.inviteUser(name, email, userGroup, message);
		this._showPostInvite = true;
	}

	private _resetForm() {
		this._showPostInvite = false;
		this._invitedUser = undefined;
	}

	private _goToProfile() {
		//TODO: navigate to user profile
	}

	private _renderForm() {
		return html` <h1>Invite user</h1>
			<p style="margin-top: 0">
				Invite new users to give them access to Umbraco. An invite email will be sent to the user with information on
				how to log in to Umbraco. Invites last for 72 hours.
			</p>
			<uui-form>
				<form id="invite-form" name="invite-form" @submit="${this._handleSubmit}">
					<uui-form-layout-item>
						<uui-label slot="label" for="name" required>Name</uui-label>
						<uui-input id="name" type="text" name="name" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label slot="label" for="email" required>Email</uui-label>
						<uui-input id="email" type="email" name="email" required></uui-input>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label slot="label" for="userGroup" required>User group</uui-label>
						<span slot="description">Add groups to assign access and permissions</span>
						<b>ADD USER GROUP PICKER HERE</b>
					</uui-form-layout-item>
					<uui-form-layout-item>
						<uui-label slot="label" for="message" required>Message</uui-label>
						<uui-textarea id="message" name="message" required></uui-textarea>
					</uui-form-layout-item>
					<uui-button style="margin-left: auto" type="submit" label="Send Invite" look="primary"></uui-button>
				</form>
			</uui-form>`;
	}

	private _renderPostInvite() {
		if (!this._invitedUser) return nothing;

		return html`<div id="post-invite">
			<h1><b style="color: var(--uui-color-interactive-emphasis)">${this._invitedUser.name}</b> has been invited</h1>
			<p>An invitation has been sent to the new user with details about how to log in to Umbraco.</p>
			<div id="post-invite-buttons">
				<uui-button label="Invite another user" look="secondary" @click="${this._resetForm}"></uui-button>
				<uui-button
					style="margin-left: auto"
					label="Go to profile"
					look="primary"
					@click="${this._goToProfile}"></uui-button>
			</div>
		</div>`;
	}

	render() {
		return html`<uui-box>${this._showPostInvite ? this._renderPostInvite() : this._renderForm()}</uui-box>`;
	}
}

export default UmbEditorViewUsersInviteElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-invite': UmbEditorViewUsersInviteElement;
	}
}
