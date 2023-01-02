import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing, PropertyValueMap } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbInputListBase } from '../../../backoffice/core/components/input-list-base/input-list-base';
import type { UserEntity } from '@umbraco-cms/models';
import { UmbUserStore } from 'src/auth/users/user.store';
import { UmbContextConsumerController } from 'src/core/context-api/consume/context-consumer.controller';
import { UmbControllerHostMixin } from 'src/core/controller/controller-host.mixin';
import { UmbObserverController } from 'src/core/observable-api/observer.controller';

@customElement('umb-input-user')
export class UmbPickerUserElement extends UmbControllerHostMixin(UmbInputListBase) {
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
		new UmbContextConsumerController(this, 'umbUserStore', (userStore: UmbUserStore) => {
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

		new UmbObserverController<Array<UserEntity>>(this, this._userStore.getByKeys(this.value), (users) => {
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
