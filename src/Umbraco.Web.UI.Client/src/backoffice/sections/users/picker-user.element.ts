import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { UserDetails, UserEntity } from '../../../core/models';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import './picker.element';
import { UmbPickerChangedEvent } from './picker.element';

@customElement('umb-picker-user')
export class UmbPickerUserElement extends UmbContextConsumerMixin(LitElement) {
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
	private _userKeys: Array<string> = [];

	@state()
	private _users: Array<UserEntity> = [];

	protected _userStore?: UmbUserStore;
	protected _usersSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (usersContext: UmbUserStore) => {
			this._userStore = usersContext;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._usersSubscription?.unsubscribe();
	}

	private _handleSelection(e: UmbPickerChangedEvent) {
		this._userKeys = e.target.value;
		this._observeUser();
	}

	private _handleRemove(key: string) {
		this._userKeys = this._userKeys.filter((k) => k !== key);
		this._observeUser();
	}

	private _observeUser() {
		this._usersSubscription?.unsubscribe();
		this._usersSubscription = this._userStore?.getByKeys(this._userKeys).subscribe((users) => {
			this._users = users;
		});
	}

	private _renderUserList() {
		if (this._users.length === 0) return nothing;

		return html`<div id="user-list">
			${this._users.map(
				(user) => html`
					<div class="user">
						<uui-avatar .name=${user.name}></uui-avatar>
						<div>${user.name}</div>
						<uui-button @click=${() => this._handleRemove(user.key)} label="remove"></uui-button>
					</div>
				`
			)}
		</div> `;
	}

	render() {
		return html`
			<umb-picker .picker=${'user'} .value=${this._userKeys} @changed=${this._handleSelection}></umb-picker>
			${this._renderUserList()}
		`;
	}
}

export default UmbPickerUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-user': UmbPickerUserElement;
	}
}
