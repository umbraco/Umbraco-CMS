import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import { UmbWorkspaceUserContext } from '../user-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-workspace-action-user-save')
export class UmbWorkspaceActionUserSaveElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _workspaceContext?: UmbWorkspaceUserContext;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceUserContext>('umbWorkspaceContext', (instance) => {
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
}

export default UmbWorkspaceActionUserSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-user-save': UmbWorkspaceActionUserSaveElement;
	}
}
