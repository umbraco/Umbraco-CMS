import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbNotificationDefaultData } from '../../../../../core/services/notification/layouts/default';
import type { UmbNotificationService } from '../../../../../core/services/notification';
import { UmbDocumentTypeStore } from '../../../../../core/stores/document-type/document-type.store';
import { UmbDocumentTypeContext } from '../../document-type.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-workspace-action-document-type-save')
export class UmbWorkspaceActionDocumentTypeSaveElement extends UmbContextConsumerMixin(LitElement) {
	
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _documentTypeStore?: UmbDocumentTypeStore;
	private _documentTypeContext?: UmbDocumentTypeContext;
	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeAllContexts(
			['umbDocumentTypeStore', 'umbDocumentTypeContext', 'umbNotificationService'],
			(instances) => {
				this._documentTypeStore = instances['umbDocumentTypeStore'];
				this._documentTypeContext = instances['umbDocumentTypeContext'];
				this._notificationService = instances['umbNotificationService'];
			}
		);
	}

	private async _handleSave() {
		if (!this._documentTypeStore || !this._documentTypeContext) return;

		try {
			this._saveButtonState = 'waiting';
			const dataType = this._documentTypeContext.getData();
			await this._documentTypeStore.save([dataType]);
			const data: UmbNotificationDefaultData = { message: 'Document Type Saved' };
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

export default UmbWorkspaceActionDocumentTypeSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-document-type-save': UmbWorkspaceActionDocumentTypeSaveElement;
	}
}
