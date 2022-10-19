import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import type { UmbUserStore } from '../../../../../core/stores/user/user.store';
import { UmbSectionViewUsersElement } from './section-view-users.element';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-editor-view-users-selection')
export class UmbEditorViewUsersSelectionElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-3);
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				align-items: center;
				box-sizing: border-box;
			}
		`,
	];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _totalUsers = 0;

	private _usersContext?: UmbSectionViewUsersElement;
	private _selectionSubscription?: Subscription;
	private _userStore?: UmbUserStore;
	private _totalUsersSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		this.consumeContext('umbUsersContext', (usersContext: UmbSectionViewUsersElement) => {
			this._usersContext = usersContext;
			this._observeSelection();
		});

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
			this._observeTotalUsers();
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
			this._selection = selection;
		});
	}

	private _observeTotalUsers() {
		this._totalUsersSubscription?.unsubscribe();
		this._userStore?.totalUsers.subscribe((totalUsers: number) => {
			this._totalUsers = totalUsers;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._selectionSubscription?.unsubscribe();
		this._totalUsersSubscription?.unsubscribe();
	}

	private _handleClearSelection() {
		this._usersContext?.setSelection([]);
	}

	private _renderSelectionCount() {
		return html`<div>${this._selection.length} of ${this._totalUsers} selected</div>`;
	}

	render() {
		return html`<uui-button @click=${this._handleClearSelection} label="Clear selection" look="secondary"></uui-button>
			${this._renderSelectionCount()}
			<uui-button style="margin-left: auto" label="Set group" look="secondary"></uui-button>
			<uui-button label="Enable" look="secondary"></uui-button>
			<uui-button label="Unlock" disabled look="secondary"></uui-button>
			<uui-button label="Disable" look="secondary"></uui-button> `;
	}
}

export default UmbEditorViewUsersSelectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-selection': UmbEditorViewUsersSelectionElement;
	}
}
