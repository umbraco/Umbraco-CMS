import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbUserStore } from '../../../../core/stores/user/user.store';
import { UmbUserContext } from '../user.context';

import { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';
import { UmbNotificationService } from '../../../../core/services/notification';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

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

		this.consumeAllContexts(['umbUserStore', 'umbUserContext', 'umbNotificationService'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._userContext = instances['umbUserContext'];
			this._notificationService = instances['umbNotificationService'];
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
