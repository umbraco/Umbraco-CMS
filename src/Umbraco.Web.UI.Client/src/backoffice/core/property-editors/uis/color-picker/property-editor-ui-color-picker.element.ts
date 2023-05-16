import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UUIColorSwatchesEvent } from '@umbraco-ui/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';

/**
 * @element umb-property-editor-ui-color-picker
 */
@customElement('umb-property-editor-ui-color-picker')
export class UmbPropertyEditorUIColorPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _showLabels = false;

	@state()
	private _swatches: UmbSwatchDetails[] = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const useLabel = config.find((x) => x.alias === 'useLabel');
		if (useLabel) this._showLabels = useLabel.value;

		const colorSwatches = config.find((x) => x.alias === 'items');
		if (colorSwatches) this._swatches = colorSwatches.value as any[];
	}

	private _onChange(event: UUIColorSwatchesEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-color-picker
			@change="${this._onChange}"
			.swatches="${this._swatches}"
			.showLabels="${this._showLabels}"></umb-input-color-picker>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
