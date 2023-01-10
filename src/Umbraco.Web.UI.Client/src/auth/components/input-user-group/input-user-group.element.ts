import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbInputListBase } from '../../../backoffice/shared/components/input-list-base/input-list-base';
import type { UserGroupEntity } from '@umbraco-cms/models';
import { UmbUserGroupStore } from 'src/backoffice/users/user-groups/user-group.store';

@customElement('umb-input-user-group')
export class UmbInputPickerUserGroupElement extends UmbInputListBase {
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
			.user-group div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-4);
			}
			.user-group uui-button {
				margin-left: auto;
			}
		`,
	];

	@state()
	private _userGroups: Array<UserGroupEntity> = [];

	private _userGroupStore?: UmbUserGroupStore;

	connectedCallback(): void {
		super.connectedCallback();
		this.pickerLayout = 'umb-picker-layout-user-group';
		this.consumeContext('umbUserGroupStore', (usersContext: UmbUserGroupStore) => {
			this._userGroupStore = usersContext;
			this._observeUserGroups();
		});
	}

	private _observeUserGroups() {
		if (this.value.length > 0 && this._userGroupStore) {
			this.observe(
				this._userGroupStore.getByKeys(this.value),
				(userGroups) => (this._userGroups = userGroups)
			);
		} else {
			this._userGroups = [];
		}
	}

	selectionUpdated() {
		this._observeUserGroups();
		this.dispatchEvent(new CustomEvent('property-value-change', { bubbles: true, composed: true }));
	}

	private _renderUserGroupList() {
		if (this._userGroups.length === 0) return nothing;

		return html`<div id="user-list">
			${this._userGroups.map(
				(userGroup) => html`
					<div class="user-group">
						<div>
							<uui-icon .name=${userGroup.icon}></uui-icon>
							<span>${userGroup.name}</span>
						</div>
						<uui-button
							@click=${() => this.removeFromSelection(userGroup.key)}
							label="remove"
							color="danger"></uui-button>
					</div>
				`
			)}
		</div> `;
	}

	renderContent() {
		return html`${this._renderUserGroupList()}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-user-group': UmbInputPickerUserGroupElement;
	}
}
