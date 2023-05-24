import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import type { UmbInputDocumentPickerElement } from '../../components/input-document-picker/input-document-picker.element.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIContentPickerElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		const validationLimit = config.find((x) => x.alias === 'validationLimit');

		this._limitMin = (validationLimit?.value as any).min;
		this._limitMax = (validationLimit?.value as any).max;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputDocumentPickerElement).selectedIds;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	// TODO: Implement mandatory?
	render() {
		return html`
			<umb-input-document-picker
				@change=${this._onChange}
				.selectedIds=${this._value}
				.min=${this._limitMin}
				.max=${this._limitMax}
				>Add</umb-input-document-picker
			>
		`;
	}
}

export default UmbPropertyEditorUIContentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIContentPickerElement;
	}
}
