import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import UmbSectionViewUsersElement from '../../section-view-users.element';
import { UmbUserStore } from '../../../../../../../core/stores/user/user.store';
import { getTagLookAndColor } from '../../../../user-extensions';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UserDetails, UserEntity } from '@umbraco-cms/models';
import { umbHistoryService } from 'src/core/services/history';

@customElement('umb-editor-view-users-grid')
export class UmbEditorViewUsersGridElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}

			#user-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-4);
				padding-top: 0;
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

	private _userStore?: UmbUserStore;
	private _usersContext?: UmbSectionViewUsersElement;
	private _usersSubscription?: Subscription;
	private _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		umbHistoryService.push({ label: 'Users grid', path: 'section/users/view/users/overview/grid' });

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeUsers();
		});

		this.consumeContext('umbUsersContext', (usersContext: UmbSectionViewUsersElement) => {
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
		history.pushState(null, '', 'section/users/view/users/user/' + key); //TODO Change to a tag with href and make dynamic
	}

	private _selectRowHandler(user: UserEntity) {
		this._usersContext?.select(user.key);
	}

	private _deselectRowHandler(user: UserEntity) {
		this._usersContext?.deselect(user.key);
	}

	private renderUserCard(user: UserDetails) {
		if (!this._userStore) return;

		const statusLook = getTagLookAndColor(user.status);

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
			<uui-scroll-container>
				<div id="user-grid">
					${repeat(
						this._users,
						(user) => user.key,
						(user) => this.renderUserCard(user)
					)}
				</div>
			</uui-scroll-container>
		`;
	}
}

export default UmbEditorViewUsersGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-grid': UmbEditorViewUsersGridElement;
	}
}
