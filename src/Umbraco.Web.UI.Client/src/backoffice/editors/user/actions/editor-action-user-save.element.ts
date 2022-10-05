import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../core/context';
import { UmbUserStore } from '../../../../core/stores/user/user.store';
import type { UserEntity } from '../../../../core/models';
import { UmbUserContext } from '../user.context';
import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';
import { UmbNotificationService } from '../../../../core/services/notification';

@customElement('umb-editor-action-user-save')
export class UmbEditorActionUserSaveElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _userStore?: UmbUserStore;
	private _userContext?: UmbUserContext;
	private _notificationService?: UmbNotificationService;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
		});

		this.consumeContext('umbUserContext', (userContext: UmbUserContext) => {
			this._userContext = userContext;
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});
	}

	private async _handleSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._userStore || !this._userContext) return;

		try {
			this._saveButtonState = 'waiting';
			const user = this._userContext.getData();
			await this._userStore.save([user]);
			const data: UmbNotificationDefaultData = { message: 'User Saved' };
			this._notificationService?.peek('positive', { data });
			this._saveButtonState = 'success';
		} catch (error) {
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

export default UmbEditorActionUserSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-action-user-save': UmbEditorActionUserSaveElement;
	}
}
