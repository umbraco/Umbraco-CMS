import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '../../../core/context';
import UmbSectionViewUsersElement from '../../sections/users/views/users/section-view-users.element';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import type { UserDetails } from '../../../core/models';
import { UmbUserContext } from './user.context';
import '../../property-editor-uis/content-picker/property-editor-ui-content-picker.element';

import '../shared/editor-entity-layout/editor-entity-layout.element';
import { getTagLookAndColor } from '../../sections/users/user-extensions';
@customElement('umb-editor-user')
export class UmbEditorUserElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#main {
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

	@state()
	private _userName = '';

	@property({ type: String })
	entityKey = '';

	protected _userStore?: UmbUserStore;
	protected _usersSubscription?: Subscription;
	private _userContext?: UmbUserContext;

	private _userNameSubscription?: Subscription;

	private _languages = []; //TODO Add languages

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (usersContext: UmbUserStore) => {
			this._userStore = usersContext;
			this._observeUser();
		});
	}

	private _observeUser() {
		this._usersSubscription?.unsubscribe();

		this._usersSubscription = this._userStore?.getByKey(this.entityKey).subscribe((user) => {
			this._user = user;
			if (!this._user) return;

			if (!this._userContext) {
				this._userContext = new UmbUserContext(this._user);
				this.provideContext('umbUserContext', this._userContext);
			} else {
				this._userContext.update(this._user);
			}

			this._userNameSubscription = this._userContext.data.subscribe((user) => {
				if (user && user.name !== this._userName) {
					this._userName = user.name;
				}
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
		this._userNameSubscription?.unsubscribe();
	}

	private _updateUserStatus() {
		if (!this._user || !this._userStore) return;

		const isDisabled = this._user.status === 'disabled';
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
					<uui-input name="email" label="email" readonly value=${this._user.email}></uui-input>
				</uui-form-layout-item>
				<uui-form-layout-item style="margin-bottom: 0">
					<uui-label for="language">Language</uui-label>
					<uui-select name="language" label="language" .options=${this._languages}> </uui-select>
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
						<umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker>
					</div>
					<div>
						<b>Media start nodes</b>
						<div class="faded-text">Limit the media library to specific start nodes</div>
						<umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker>
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

		const statusLook = getTagLookAndColor(this._user.status);

		return html` <uui-box>
			<div id="user-info">
				<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
				<uui-button label="Change photo"></uui-button>
				<hr />
				${this._user?.status !== 'invited'
					? html`
							<uui-button
								@click=${this._updateUserStatus}
								look="primary"
								color="${this._user.status === 'disabled' ? 'positive' : 'warning'}"
								label="${this._user.status === 'disabled' ? 'Enable' : 'Disable'}"></uui-button>
					  `
					: nothing}
				<uui-button @click=${this._deleteUser} look="primary" color="danger" label="Delete User"></uui-button>
				<div>
					<b>Status:</b>
					<uui-tag look="${ifDefined(statusLook?.look)}" color="${ifDefined(statusLook?.color)}">
						${this._user.status}
					</uui-tag>
				</div>
				${this._user?.status === 'invited'
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

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._userContext?.update({ name: target.value });
			}
		}
	}

	render() {
		if (!this._user) return html`User not found`;

		return html`
			<umb-editor-entity-layout alias="Umb.Editor.User">
				<uui-input id="name" slot="name" .value=${this._userName} @input="${this._handleInput}"></uui-input>
				<div id="main">
					<div id="left-column">${this.renderLeftColumn()}</div>
					<div id="right-column">${this.renderRightColumn()}</div>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-user-details': UmbEditorUserElement;
	}
}
