import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbInputMediaPickerElement } from '../../../../media/media/components/input-media-picker/input-media-picker.element.js';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
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
		const validationLimit = config.getByAlias('validationLimit');
		if (!validationLimit) return;

		const minMax: Record<string, number> = validationLimit.value;

		this._limitMin = minMax.min;
		this._limitMax = minMax.max;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMediaPickerElement).selectedIds;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<umb-input-media-picker
				@change=${this._onChange}
				.selectedIds=${this._value}
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
