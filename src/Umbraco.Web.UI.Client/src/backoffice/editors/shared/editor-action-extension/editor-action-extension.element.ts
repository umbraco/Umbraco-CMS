import { UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { createExtensionElement } from '../../../../core/extension';
import type { ManifestEditorAction } from '../../../../core/models';

@customElement('umb-editor-action-extension')
export class UmbEditorActionExtensionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _editorAction?: ManifestEditorAction;
	@property({ type: Object })
	public get editorAction(): ManifestEditorAction | undefined {
		return this._editorAction;
	}
	public set editorAction(value: ManifestEditorAction | undefined) {
		this._editorAction = value;
		this._createElement();
	}

	@state()
	private _element?: any;

	private async _createElement() {
		if (!this.editorAction) return;

		try {
			this._element = await createExtensionElement(this.editorAction);
			if (!this._element) return;

			this._element.editorAction = this.editorAction;
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
		'umb-editor-action-extension': UmbEditorActionExtensionElement;
	}
}
