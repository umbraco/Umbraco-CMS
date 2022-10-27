import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { UserEntity } from '../../../core/models';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbPickerElement } from './picker.element';
import './picker.element';

@customElement('umb-picker-user')
export class UmbPickerUserElement extends UmbObserverMixin(UmbPickerElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#user-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.user {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
			.user uui-button {
				margin-left: auto;
			}
		`,
	];

	@state()
	private _users: Array<UserEntity> = [];

	private _userStore?: UmbUserStore;

	connectedCallback(): void {
		super.connectedCallback();
		this.pickerLayout = 'umb-picker-layout-user';
		this.consumeContext('umbUserStore', (usersContext: UmbUserStore) => {
			this._userStore = usersContext;
		});
	}

	private _observeUser() {
		if (this.value.length > 0 && this._userStore) {
			this.observe<Array<UserEntity>>(this._userStore.getByKeys(this.value), (users) => (this._users = users));
		} else {
			this._users = [];
		}
	}

	selectionUpdated() {
		this._observeUser();
	}

	private _renderUserList() {
		if (this._users.length === 0) return nothing;

		return html`<div id="user-list">
			${this._users.map(
				(user) => html`
					<div class="user">
						<uui-avatar .name=${user.name}></uui-avatar>
						<div>${user.name}</div>
						<uui-button @click=${() => this.removeFromSelection(user.key)} label="remove" color="danger"></uui-button>
					</div>
				`
			)}
		</div> `;
	}

	renderContent() {
		return html`${this._renderUserList()}`;
	}
}

export default UmbPickerUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-user': UmbPickerUserElement;
	}
}
