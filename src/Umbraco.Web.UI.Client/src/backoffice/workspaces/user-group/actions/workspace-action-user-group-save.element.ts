import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';
import type { UmbNotificationService } from '../../../../core/services/notification';
import { UmbUserGroupContext } from '../user-group.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbUserGroupStore } from 'src/core/stores/user/user-group.store';
import { UmbUserStore } from 'src/core/stores/user/user.store';

@customElement('umb-workspace-action-user-group-save')
export class UmbWorkspaceActionUserGroupSaveElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _userGroupStore?: UmbUserGroupStore;
	private _userStore?: UmbUserStore;
	private _userGroupContext?: UmbUserGroupContext;
	private _notificationService?: UmbNotificationService;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeAllContexts(
			['umbUserGroupStore', 'umbUserStore', 'umbUserGroupContext', 'umbNotificationService'],
			(instances) => {
				this._userGroupStore = instances['umbUserGroupStore'];
				this._userStore = instances['umbUserStore'];
				this._userGroupContext = instances['umbUserGroupContext'];
				this._notificationService = instances['umbNotificationService'];
			}
		);
	}

	private async _handleSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._userGroupStore || !this._userGroupContext) return;

		try {
			this._saveButtonState = 'waiting';
			const userGroup = this._userGroupContext.getData();
			await this._userGroupStore.save([userGroup]);
			if (this._userStore && userGroup.users) {
				await this._userStore.updateUserGroup(userGroup.users, userGroup.key);
			}

			const notificationData: UmbNotificationDefaultData = { message: 'User Group Saved' };
			this._notificationService?.peek('positive', { data: notificationData });
			this._saveButtonState = 'success';
		} catch (error) {
			const notificationData: UmbNotificationDefaultData = { message: 'User Group Save Failed' };
			this._notificationService?.peek('danger', { data: notificationData });
			this._saveButtonState = 'failed';
		}
	}

	render() {
		return html`<uui-button
			@click=${this._handleSave}
			look="primary"
			color="positive"
			label="save"
			.state="${this._saveButtonState}"></uui-button>`;
	}
}

export default UmbWorkspaceActionUserGroupSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-user-group-save': UmbWorkspaceActionUserGroupSaveElement;
	}
}
