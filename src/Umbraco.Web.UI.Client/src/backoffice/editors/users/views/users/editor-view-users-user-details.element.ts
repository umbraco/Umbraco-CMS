import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import UmbEditorViewUsersElement, { UserItem } from './editor-view-users.element';
import { Subscription } from 'rxjs';

@customElement('umb-editor-view-users-user-details')
export class UmbEditorViewUsersUserDetailsElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _users: Array<UserItem> = [];

	@state()
	private _user?: UserItem;

	protected _usersContext?: UmbEditorViewUsersElement;
	protected _usersSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUsersContext', (usersContext: UmbEditorViewUsersElement) => {
			this._usersContext = usersContext;

			this._usersSubscription?.unsubscribe();
			this._usersSubscription = this._usersContext?.users.subscribe((users: Array<UserItem>) => {
				this._users = users;
			});
		});

		// get user id from url path
		const path = window.location.pathname;
		const pathParts = path.split('/');
		const userKey = pathParts[pathParts.length - 1];

		// get user from users array
		this._user = this._users.find((user: UserItem) => user.key === userKey);
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._usersSubscription?.unsubscribe();
	}

	render() {
		return html`${this._user?.name}`;
	}
}

export default UmbEditorViewUsersUserDetailsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-user-details': UmbEditorViewUsersUserDetailsElement;
	}
}
