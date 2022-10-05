import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbContextConsumerMixin } from '../../../../../../../core/context';
import UmbEditorViewUsersElement from '../../editor-view-users.element';
import { UmbUserStore } from '../../../../../../../core/stores/user/user.store';
import type { UserEntity } from '../../../../../../../core/models';

@customElement('umb-editor-view-users-grid')
export class UmbEditorViewUsersGridElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#user-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
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
	private _users: Array<UserEntity> = [];

	@state()
	private _selection: Array<string> = [];

	private _userStore?: UmbUserStore;
	private _usersContext?: UmbEditorViewUsersElement;
	private _usersSubscription?: Subscription;
	private _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeUsers();
		});

		this.consumeContext('umbUsersContext', (usersContext: UmbEditorViewUsersElement) => {
			this._usersContext = usersContext;
			this._observeSelection();
		});
	}

	private _observeUsers() {
		this._usersSubscription?.unsubscribe();
		this._usersSubscription = this._userStore?.getAll().subscribe((users) => {
			this._users = users;
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
			this._selection = selection;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
		this._selectionSubscription?.unsubscribe();
	}

	private _isSelected(key: string) {
		return this._selection.includes(key);
	}

	//TODO How should we handle url stuff?
	private _handleOpenCard(key: string) {
		history.pushState(null, '', '/section/users/view/users/details' + '/' + key); //TODO Change to a tag with href and make dynamic
	}

	private _selectRowHandler(user: UserEntity) {
		this._usersContext?.select(user.key);
	}

	private _deselectRowHandler(user: UserEntity) {
		this._usersContext?.deselect(user.key);
	}

	private renderUserCard(user: UserEntity) {
		if (!this._userStore) return;

		const statusLook = this._usersContext?.getTagLookAndColor(user.status ? user.status : '');

		return html`
			<uui-card-user
				.name=${user.name}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this._isSelected(user.key)}
				@open=${() => this._handleOpenCard(user.key)}
				@selected=${() => this._selectRowHandler(user)}
				@unselected=${() => this._deselectRowHandler(user)}>
				${user.status && user.status !== 'Enabled'
					? html`<uui-tag
							slot="tag"
							size="s"
							look="${ifDefined(statusLook?.look)}"
							color="${ifDefined(statusLook?.color)}">
							${user.status}
					  </uui-tag>`
					: nothing}
				<div>USER GROUPS NOT IMPLEMENTED</div>
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

export default UmbEditorViewUsersGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-grid': UmbEditorViewUsersGridElement;
	}
}
