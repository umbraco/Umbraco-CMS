import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing, PropertyValueMap } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbInputListBase } from '../input-list-base/input-list-base';
import { UmbUserStore, UMB_USER_STORE_CONTEXT_TOKEN } from '../../../users/users/repository/user.store';
import type { UserEntity } from '@umbraco-cms/models';
import { UMB_USER_PICKER_MODAL_TOKEN } from 'src/backoffice/users/users/modals/user-picker';

@customElement('umb-input-user')
export class UmbPickerUserElement extends UmbInputListBase {
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
		this.pickerToken = UMB_USER_PICKER_MODAL_TOKEN;
		this.consumeContext(UMB_USER_STORE_CONTEXT_TOKEN, (userStore) => {
			this._userStore = userStore;
			this._observeUser();
		});
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('value')) {
			this._observeUser(); // TODO: This works, but it makes the value change twice.
		}
	}

	private _observeUser() {
		if (!this._userStore) return;

		// TODO: Fix type mismatch:
		this.observe<Array<UserEntity>>(this._userStore.getByKeys(this.value), (users) => {
			this._users = users;
		});
	}

	selectionUpdated() {
		this._observeUser();
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-user': UmbPickerUserElement;
	}
}
