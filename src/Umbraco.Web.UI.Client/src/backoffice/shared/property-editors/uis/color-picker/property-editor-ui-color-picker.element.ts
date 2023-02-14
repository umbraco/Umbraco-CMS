import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UUIColorSwatchesEvent } from '@umbraco-ui/uui';
import '../../../../shared/components/color-picker/color-picker.element';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DataTypePropertyModel } from '@umbraco-cms/backend-api';

/**
 * @element umb-property-editor-ui-color-picker
 */
@customElement('umb-property-editor-ui-color-picker')
export class UmbPropertyEditorUIColorPickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@state()
	private _includeLabels = false;

	@state()
	private _colorSwatches: string[] = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyModel>) {
		const includeLabels = config.find((x) => x.alias === 'includeLabels');
		if (includeLabels) this._includeLabels = includeLabels.value;

		const colorSwatches = config.find((x) => x.alias === 'colors');
		if (colorSwatches) this._colorSwatches = colorSwatches.value;
	}

	private _onChange(event: UUIColorSwatchesEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-color-picker
			@change="${this._onChange}"
			.colors="${this._colorSwatches}"
			.showLabels="${this._includeLabels}"></umb-color-picker>`;
	}
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
