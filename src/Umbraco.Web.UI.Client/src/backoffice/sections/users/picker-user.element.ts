import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { UserDetails, UserEntity } from '../../../core/models';
import { UmbUserStore } from '../../../core/stores/user/user.store';
import './picker.element';
import { UmbPickerChangedEvent } from './picker.element';

@customElement('umb-picker-user')
export class UmbPickerUserElement extends UmbContextConsumerMixin(LitElement) {
	private _handleSelection(e: UmbPickerChangedEvent) {
		console.clear();
		console.log('handle selection done', e.target.value);

		this._userKeys = e.target.value;
		console.log('user keys', this._userKeys);
		this._observeUser();
	}

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

	private _observeUser() {
		this._usersSubscription?.unsubscribe();
		this._usersSubscription = this._userStore?.getByKeys(this._userKeys).subscribe((users) => {
			this._users = users;
		});
	}

	render() {
		return html`<umb-picker .picker=${'user'} .value=${this._userKeys} @changed=${this._handleSelection}></umb-picker>
			<div>
				${this._users.map(
					(user) => html`
						<div>
							<div>${user.name}</div>
							<div>${user.key}</div>
						</div>
					`
				)}
			</div> `;
	}
}

export default UmbPickerUserElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-user': UmbPickerUserElement;
	}
}
