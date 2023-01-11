import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ChangeEvent } from 'react';
import { UmbLitElement } from '@umbraco-cms/element';
// eslint-disable-next-line import/no-named-as-default
import type { UmbInputDocumentPickerElement } from 'src/backoffice/shared/components/input-document-picker/input-document-picker.element';
import 'src/backoffice/shared/components/input-document-picker/input-document-picker.element';

// TODO: rename to Document Picker
@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIContentPickerElement extends UmbLitElement {
	/*
	static styles = [
		UUITextStyles,
		css`

		`,
	];
	*/

	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		console.log("property editor got value", value)
			this._value = value || [];
	}

	// TODO: Use config for something.
	@property({ type: Array, attribute: false })
	public config = [];


	private _onChange(event: ChangeEvent) {
		this.value = (event.target as UmbInputDocumentPickerElement).selectedKeys;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}



	// TODO: Implement mandatory?


	render() {
		return html`
			<umb-input-document-picker @change=${this._onChange} .selectedKeys=${this._value}>Add</umb-input-document-picker>
		`;
	}

}

export default UmbPropertyEditorUIContentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
