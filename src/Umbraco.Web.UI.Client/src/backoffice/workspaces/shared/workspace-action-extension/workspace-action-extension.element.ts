import { UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestWorkspaceAction } from '@umbraco-cms/models';

// Niels, thoughts, TODO: Consider naming this just actions, not extension as everything is an extension.
@customElement('umb-workspace-action-extension')
export class UmbWorkspaceActionExtensionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _workspaceAction?: ManifestWorkspaceAction;
	@property({ type: Object })
	public get workspaceAction(): ManifestWorkspaceAction | undefined {
		return this._workspaceAction;
	}
	public set workspaceAction(value: ManifestWorkspaceAction | undefined) {
		this._workspaceAction = value;
		this._createElement();
	}

	@state()
	private _element?: any;

	private async _createElement() {
		if (!this.workspaceAction) return;

		try {
			this._element = await createExtensionElement(this.workspaceAction);
			if (!this._element) return;

			this._element.editorAction = this.workspaceAction;
		} catch (error) {
			// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
		}
	}

	render() {
		return html`${this._element}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-extension': UmbWorkspaceActionExtensionElement;
	}
}
