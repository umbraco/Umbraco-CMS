import type UmbInputTiptapElement from '../../components/input-tiptap/input-tiptap.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

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
	public set value(value: string) {
		this._value = value;
	}
	public get value(): string {
		return this._value;
	}

	@state()
	_config?: UmbPropertyEditorConfigCollection;

	@state()
	private _value: string = '';

	#onChange(event: CustomEvent & { target: UmbInputTiptapElement }) {
		const value = event.target.value as string;
		this._value = value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<umb-input-tiptap
			.value=${this.value}
			@change=${this.#onChange}
			.configuration=${this._config}></umb-input-tiptap>`;
	}
}

export default UmbPropertyEditorUITiptapElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap': UmbPropertyEditorUITiptapElement;
	}
}
