import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbInputMediaPickerElement } from '../../../../../backoffice/shared/components/input-media-picker/input-media-picker.element';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DataTypePropertyModel } from '@umbraco-cms/backend-api';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbLitElement implements UmbPropertyEditorElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		const validationLimit = config.find((x) => x.alias === 'validationLimit');
		if (!validationLimit) return;

		this._limitMin = (validationLimit?.value as any).min;
		this._limitMax = (validationLimit?.value as any).max;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMediaPickerElement).selectedKeys;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<umb-input-media-picker
				@change=${this._onChange}
				.selectedKeys=${this._value}
				.min=${this._limitMin}
				.max=${this._limitMax}
				>Add</umb-input-media-picker
			>
		`;
	}
}

export default UmbPropertyEditorUIMediaPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
