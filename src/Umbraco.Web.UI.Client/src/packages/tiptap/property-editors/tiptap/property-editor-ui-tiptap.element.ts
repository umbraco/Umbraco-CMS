import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-tiptap/input-tiptap.element.js';

// Look at Tiny for correct types
export interface UmbRichTextEditorValueType {
	markup?: string;
	blocks?: any;
}

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement('umb-property-editor-ui-tiptap')
export class UmbPropertyEditorUITiptapElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._config = config;
	}

	@property({ attribute: false })
	public set value(value: UmbRichTextEditorValueType) {
		this._value = value;
	}
	public get value(): UmbRichTextEditorValueType {
		return this._value;
	}

	@state()
	_config?: UmbPropertyEditorConfigCollection;

	@state()
	private _value: UmbRichTextEditorValueType = {};

	override render() {
		return html`<umb-input-tiptap></umb-input-tiptap>`;
	}
}

export default UmbPropertyEditorUITiptapElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap': UmbPropertyEditorUITiptapElement;
	}
}
