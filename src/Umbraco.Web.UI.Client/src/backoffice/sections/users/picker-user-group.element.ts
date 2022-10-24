import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import type { UserGroupEntity } from '../../../core/models';
import { UmbUserGroupStore } from '../../../core/stores/user/user-group.store';
import './picker.element';
import UmbPickerElement from './picker.element';

@customElement('umb-picker-user-group')
export class UmbPickerUserGroupElement extends UmbPickerElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#user-group-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			.user-group {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
			.user-group uui-button {
				margin-left: auto;
			}
		`,
	];

	@state()
	private _userGroups: Array<UserGroupEntity> = [];

	private _userGroupStore?: UmbUserGroupStore;
	private _userGroupsSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		this.pickerLayout = 'umb-picker-layout-user-group';
		this.consumeContext('umbUserGroupStore', (usersContext: UmbUserGroupStore) => {
			this._userGroupStore = usersContext;
			this._observeUserGroups();
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._userGroupsSubscription?.unsubscribe();
	}

	private _observeUserGroups() {
		this._userGroupsSubscription?.unsubscribe();

		if (this.selection.length > 0) {
			this._userGroupsSubscription = this._userGroupStore?.getByKeys(this.selection).subscribe((userGroups) => {
				this._userGroups = userGroups;
			});
		} else {
			this._userGroups = [];
		}
	}

	selectionUpdated() {
		this._observeUserGroups();
	}

	private _renderUserGroupList() {
		if (this._userGroups.length === 0) return nothing;

		return html`<div id="user-list">
			${this._userGroups.map(
				(userGroup) => html`
					<div class="user-group">
						<div>${userGroup.name}</div>
						<uui-button @click=${() => this.removeFromSelection(userGroup.key)} label="remove"></uui-button>
					</div>
				`
			)}
		</div> `;
	}

	renderContent() {
		return html`${this._renderUserGroupList()}`;
	}
}

export default UmbPickerUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-user-group': UmbPickerUserGroupElement;
	}
}
