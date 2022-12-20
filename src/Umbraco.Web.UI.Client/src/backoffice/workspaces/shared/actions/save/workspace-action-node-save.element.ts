import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbWorkspaceNodeContext } from '../../workspace-context/workspace-node.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-workspace-action-document-type-save')
export class UmbWorkspaceActionNodeSaveElement extends UmbContextConsumerMixin(LitElement) {
	
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _workspaceContext?: UmbWorkspaceNodeContext;

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

export default UmbWorkspaceActionNodeSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-node-save': UmbWorkspaceActionNodeSaveElement;
	}
}
