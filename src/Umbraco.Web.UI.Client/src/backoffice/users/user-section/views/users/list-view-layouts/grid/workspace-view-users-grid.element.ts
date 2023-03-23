import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import type { UmbSectionViewUsersElement } from '../../section-view-users.element';
import {
	UmbUserGroupStore,
	UMB_USER_GROUP_STORE_CONTEXT_TOKEN,
} from '../../../../../user-groups/repository/user-group.store';
import { getLookAndColorFromUserStatus } from '@umbraco-cms/backoffice/utils';
import type { UserDetails, UserEntity, UserGroupEntity } from '@umbraco-cms/backoffice/models';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-workspace-view-users-grid')
export class UmbWorkspaceViewUsersGridElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#user-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
				margin: var(--uui-size-layout-1);
				margin-top: var(--uui-size-space-2);
			}

			uui-card-user {
				width: 100%;
				height: 180px;
			}

			.user-login-time {
				margin-top: auto;
			}
		`,
	];

	@state()
	private _users: Array<UserDetails> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _userGroups: Array<UserGroupEntity> = [];

	private _userGroupStore?: UmbUserGroupStore;
	private _usersContext?: UmbSectionViewUsersElement;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (instance) => {
			this._userGroupStore = instance;
			this._observeUserGroups();
		});

		this.consumeContext<UmbSectionViewUsersElement>('umbUsersContext', (_instance) => {
			this._usersContext = _instance;
			this._observeUsers();
			this._observeSelection();
		});
	}

	private _observeUsers() {
		if (!this._usersContext) return;
		this.observe(this._usersContext.users, (users) => {
			this._users = users;
		});
	}

	private _observeUserGroups() {
		if (!this._userGroupStore) return;
		this.observe(this._userGroupStore.getAll(), (userGroups) => (this._userGroups = userGroups));
	}

	private _observeSelection() {
		if (!this._usersContext) return;
		this.observe(this._usersContext.selection, (selection) => (this._selection = selection));
	}

	private _isSelected(key: string) {
		return this._selection.includes(key);
	}

	//TODO How should we handle url stuff?
	private _handleOpenCard(key: string) {
		history.pushState(null, '', 'section/users/view/users/user/' + key); //TODO Change to a tag with href and make dynamic
	}

	private _selectRowHandler(user: UserEntity) {
		this._usersContext?.select(user.key);
	}

	private _deselectRowHandler(user: UserEntity) {
		this._usersContext?.deselect(user.key);
	}

	private _getUserGroupNames(keys: Array<string>) {
		return keys
			.map((key: string) => {
				return this._userGroups.find((x) => x.key === key)?.name;
			})
			.join(', ');
	}

	private renderUserCard(user: UserDetails) {
		if (!this._usersContext) return;

		const statusLook = getLookAndColorFromUserStatus(user.status);

		return html`
			<uui-card-user
				.name=${user.name}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this._isSelected(user.key)}
				@open=${() => this._handleOpenCard(user.key)}
				@selected=${() => this._selectRowHandler(user)}
				@unselected=${() => this._deselectRowHandler(user)}>
				${user.status && user.status !== 'enabled'
					? html`<uui-tag
							slot="tag"
							size="s"
							look="${ifDefined(statusLook?.look)}"
							color="${ifDefined(statusLook?.color)}">
							${user.status}
					  </uui-tag>`
					: nothing}
				<div>${this._getUserGroupNames(user.userGroups)}</div>
				${user.lastLoginDate
					? html`<div class="user-login-time">
							<div>Last login</div>
							${user.lastLoginDate}
					  </div>`
					: html`<div class="user-login-time">${`${user.name} has not logged in yet`}</div>`}
			</uui-card-user>
		`;
	}

	render() {
		return html`
			<div id="user-grid">
				${repeat(
					this._users,
					(user) => user.key,
					(user) => this.renderUserCard(user)
				)}
			</div>
		`;
	}
}

export default UmbWorkspaceViewUsersGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-users-grid': UmbWorkspaceViewUsersGridElement;
	}
}
