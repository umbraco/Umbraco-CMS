import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import UmbEditorViewUsersElement from './editor-view-users.element';
import { Subscription } from 'rxjs';
import '../../../../property-editors/content-picker/property-editor-content-picker.element';
import { UmbUserStore } from '../../../../../core/stores/user/user.store';
import { UserDetails } from '../../../../../core/models';

@customElement('umb-editor-view-users-user-details')
export class UmbEditorViewUsersUserDetailsElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-space-6);
				padding: var(--uui-size-space-6);
			}

			#left-column {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#right-column > uui-box > div {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			uui-avatar {
				font-size: var(--uui-size-16);
				place-self: center;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
			uui-tag {
				width: fit-content;
			}
			#user-info {
				display: flex;
				gap: var(--uui-size-space-6);
			}
			#user-info > div {
				display: flex;
				flex-direction: column;
			}
			#assign-access {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.access-content {
				margin-top: var(--uui-size-space-1);
				margin-bottom: var(--uui-size-space-4);
				display: flex;
				align-items: center;
				line-height: 1;
				gap: var(--uui-size-space-3);
			}
			.access-content > span {
				align-self: end;
			}
		`,
	];

	@state()
	private _user?: UserDetails | null;

	@property()
	public key = '';

	protected _userStore?: UmbUserStore;
	protected _usersSubscription?: Subscription;

	private _languages = []; //TODO Add languages

	connectedCallback(): void {
		super.connectedCallback();
		this.consumeContext('umbUserStore', (usersContext: UmbUserStore) => {
			this._userStore = usersContext;

			this._usersSubscription?.unsubscribe();
			this._usersSubscription = this._userStore?.getByKey(this.key).subscribe((user) => {
				this._user = user;
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
	}

	private _updateUserStatus() {
		if (!this._user || !this._userStore) return;

		const isDisabled = this._user.status === 'Disabled';
		isDisabled ? this._userStore.enableUsers([this._user.key]) : this._userStore.disableUsers([this._user.key]);
	}

	private _deleteUser() {
		if (!this._user || !this._userStore) return;

		this._userStore.deleteUsers([this._user.key]);

		history.pushState(null, '', '/section/users/view/users/overview');
	}

	private renderLeftColumn() {
		if (!this._user) return nothing;

		return html` <uui-box>
				<div slot="headline">Profile</div>
				<uui-form-layout-item style="margin-top: 0">
					<uui-label for="email">Email</uui-label>
					<uui-input name="email" readonly value=${this._user.email}></uui-input>
				</uui-form-layout-item>
				<uui-form-layout-item style="margin-bottom: 0">
					<uui-label for="language">Language</uui-label>
					<uui-select name="language" .options=${this._languages}> </uui-select>
				</uui-form-layout-item>
			</uui-box>
			<uui-box>
				<div id="assign-access">
					<div slot="headline">Assign access</div>
					<div>
						<b>Groups</b>
						<div class="faded-text">Add groups to assign access and permissions</div>
					</div>
					<div>
						<b>Content start nodes</b>
						<div class="faded-text">Limit the content tree to specific start nodes</div>
						<umb-property-editor-content-picker></umb-property-editor-content-picker>
					</div>
					<div>
						<b>Media start nodes</b>
						<div class="faded-text">Limit the media library to specific start nodes</div>
						<umb-property-editor-content-picker></umb-property-editor-content-picker>
					</div>
				</div>
			</uui-box>
			<uui-box>
				<div slot="headline">Access</div>
				<div slot="header" class="faded-text">
					Based on the assigned groups and start nodes, the user has access to the following nodes
				</div>

				<b>Content</b>
				<div class="access-content">
					<uui-icon name="folder"></uui-icon>
					<span>Content Root</span>
				</div>

				<b>Media</b>
				<div class="access-content">
					<uui-icon name="folder"></uui-icon>
					<span>Media Root</span>
				</div>
			</uui-box>`;
	}

	private renderRightColumn() {
		if (!this._user || !this._userStore) return nothing;

		console.log('user', this._user);

		// const status = this._userStore.getTagLookAndColor(this._user.status);
		return html` <uui-box>
			<div id="user-info">
				<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
				<uui-button label="Change photo"></uui-button>
				<hr />
				${this._user?.status !== 'Invited'
					? html`
							<uui-button
								@click=${this._updateUserStatus}
								look="primary"
								color="${this._user.status === 'Disabled' ? 'positive' : 'warning'}"
								label="${this._user.status === 'Disabled' ? 'Enable' : 'Disable'}"></uui-button>
					  `
					: nothing}
				<uui-button @click=${this._deleteUser} look="primary" color="danger" label="Delete User"></uui-button>
				<div>
					<b>Status:</b>
					<uui-tag .look=${status.look} .color=${status.color}>${this._user.status}</uui-tag>
				</div>
				${this._user?.status === 'Invited'
					? html`
							<uui-textarea placeholder="Enter a message..."> </uui-textarea>
							<uui-button look="primary" label="Resend invitation"></uui-button>
					  `
					: nothing}
				<div>
					<b>Last login:</b>
					<span>${this._user.lastLoginDate || `${this._user.name} has not logged in yet`}</span>
				</div>
				<div>
					<b>Failed login attempts</b>
					<span>${this._user.failedLoginAttempts}</span>
				</div>
				<div>
					<b>Last lockout date:</b>
					<span>${this._user.lastLockoutDate || `${this._user.name} has not been locked out`}</span>
				</div>
				<div>
					<b>Password last changed:</b>
					<span>${this._user.lastLoginDate || `${this._user.name} has not changed password`}</span>
				</div>
				<div>
					<b>User created:</b>
					<span>${this._user.createDate}</span>
				</div>
				<div>
					<b>User last updated:</b>
					<span>${this._user.updateDate}</span>
				</div>
				<div>
					<b>Key:</b>
					<span>${this._user.key}</span>
				</div>
			</div>
		</uui-box>`;
	}

	render() {
		if (!this._user) return html`User not found`;

		return html`
			<div id="left-column">${this.renderLeftColumn()}</div>
			<div id="right-column">${this.renderRightColumn()}</div>
		`;
	}
}

export default UmbEditorViewUsersUserDetailsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-user-details': UmbEditorViewUsersUserDetailsElement;
	}
}
