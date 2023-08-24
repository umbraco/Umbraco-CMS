import { UMB_USER_WORKSPACE_CONTEXT } from '../user-workspace.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-user-workspace-action-save')
export class UmbUserWorkspaceActionSaveElement extends UmbLitElement {
	@state()
	private _saveButtonState?: UUIButtonState;

	private _workspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this._workspaceContext = instance;
		});
	}

	private async _handleSave() {
		if (!this._workspaceContext) return;

		this._saveButtonState = 'waiting';
		await this._workspaceContext
			.save()
			.then(() => {
				this._saveButtonState = 'success';
			})
			.catch(() => {
				this._saveButtonState = 'failed';
			});
	}

	render() {
		return html`<uui-button
			@click=${this._handleSave}
			look="primary"
			color="positive"
			label="save"
			.state="${this._saveButtonState}"></uui-button>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbUserWorkspaceActionSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-action-save': UmbUserWorkspaceActionSaveElement;
	}
}
