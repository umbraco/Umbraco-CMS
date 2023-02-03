import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUIButtonState } from '@umbraco-ui/uui';
import { UmbWorkspaceEntityContextInterface } from '../../workspace-context/workspace-entity-context.interface';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestWorkspaceAction } from '@umbraco-cms/models';

@customElement('umb-workspace-action-node-save')
export class UmbWorkspaceActionNodeSaveElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _workspaceContext?: UmbWorkspaceEntityContextInterface;

	public manifest?: ManifestWorkspaceAction;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceEntityContextInterface>('umbWorkspaceContext', (instance) => {
			this._workspaceContext = instance;
		});
	}

	private async _onSave() {
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
		return html`
			<uui-button
				@click=${this._onSave}
				look=${this.manifest?.meta.look || 'default'}
				color=${this.manifest?.meta.color || 'default'}
				label=${this.manifest?.meta.label || 'Save'}
				.state="${this._saveButtonState}"></uui-button>
		`;
	}
}

export default UmbWorkspaceActionNodeSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-node-save': UmbWorkspaceActionNodeSaveElement;
	}
}
