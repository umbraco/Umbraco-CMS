import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import { repeat } from 'lit/directives/repeat.js';
import UmbEditorViewUsersElement, { UserItem } from './editor-view-users.element';
import { Subscription } from 'rxjs';

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
	private _users: Array<UserItem> = [];

	@state()
	private _selection: Array<string> = [];

	protected _usersContext?: UmbEditorViewUsersElement;
	protected _usersSubscription?: Subscription;
	protected _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUsersContext', (usersContext: UmbEditorViewUsersElement) => {
			this._usersContext = usersContext;

			this._usersSubscription?.unsubscribe();
			this._selectionSubscription?.unsubscribe();
			this._usersSubscription = this._usersContext?.users.subscribe((users: Array<UserItem>) => {
				this._users = users;
			});
			this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
				this._selection = selection;
			});
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
		history.pushState(null, '', location.pathname + '/' + key);
	}

	private _selectRowHandler(user: UserItem) {
		this._usersContext?.select(user.key);
	}

	private _deselectRowHandler(user: UserItem) {
		this._usersContext?.deselect(user.key);
	}

	private renderUserCard(user: UserItem) {
		if (!this._usersContext) return;

		const statusLook = this._usersContext.getTagLookAndColor(user.status ? user.status : '');

		return html`
			<uui-card-user
				.name=${user.name}
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this._isSelected(user.key)}
				@open=${() => this._handleOpenCard(user.key)}
				@selected=${() => this._selectRowHandler(user)}
				@unselected=${() => this._deselectRowHandler(user)}>
				${user.status
					? html`<uui-tag slot="tag" size="s" look="${statusLook.look}" color="${statusLook.color}">
							${user.status}
					  </uui-tag>`
					: nothing}
				<div>${user.userGroup}</div>
				<div class="user-login-time">${user.lastLogin}</div>
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
