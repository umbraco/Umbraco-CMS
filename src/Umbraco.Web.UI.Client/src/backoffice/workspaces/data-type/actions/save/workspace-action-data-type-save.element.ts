import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbNotificationDefaultData } from '../../../../../core/services/notification/layouts/default';
import type { UmbNotificationService } from '../../../../../core/services/notification';
import { UmbDataTypeContext } from '../../data-type.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbDataTypesStore } from 'src/core/stores/data-types/data-types.store';

@customElement('umb-workspace-action-data-type-save')
export class UmbWorkspaceActionDataTypeSaveElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _dataTypeStore?: UmbDataTypesStore;
	private _dataTypeContext?: UmbDataTypeContext;
	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeAllContexts(['umbDataTypeStore', 'umbDataTypeContext', 'umbNotificationService'], (instances) => {
			this._dataTypeStore = instances['umbDataTypeStore'];
			this._dataTypeContext = instances['umbDataTypeContext'];
			this._notificationService = instances['umbNotificationService'];
		});
	}

	private async _handleSave() {
		if (!this._dataTypeStore || !this._dataTypeContext) return;

		try {
			this._saveButtonState = 'waiting';
			const dataType = this._dataTypeContext.getData();
			await this._dataTypeStore.save([dataType]);
			const data: UmbNotificationDefaultData = { message: 'Data Type Saved' };
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

export default UmbWorkspaceActionDataTypeSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-data-type-save': UmbWorkspaceActionDataTypeSaveElement;
	}
}
