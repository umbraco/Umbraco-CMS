import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbWorkspaceDocumentTypeContext } from '../../workspace-document-type.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-workspace-action-document-type-save')
export class UmbWorkspaceActionDocumentTypeSaveElement extends UmbContextConsumerMixin(LitElement) {
	
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (instance) => {
				this._workspaceContext = instance;
			}
		);
	}

	private async _handleSave() {
		if (!this._workspaceContext) return;

		this._saveButtonState = 'waiting';
		await this._workspaceContext.save().then(() => {
			this._saveButtonState = 'success';
		}).catch(() => {
			this._saveButtonState = 'failed';
		})
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
