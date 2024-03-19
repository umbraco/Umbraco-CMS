import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-color-picker
 */
@customElement('umb-property-editor-ui-color-picker')
export class UmbPropertyEditorUIColorPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultShowLabels = false;

	@property({ type: Object })
	value?: UmbSwatchDetails;

	@state()
	private _showLabels = this.#defaultShowLabels;

	@state()
	private _swatches: UmbSwatchDetails[] = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._showLabels = config?.getValueByAlias('useLabel') ?? this.#defaultShowLabels;
		this._swatches = config?.getValueByAlias('items') ?? [];
	}

	private _onChange(event: UUIColorSwatchesEvent) {
		const value = event.target.value;
		this.value = this._swatches.find((swatch) => swatch.value === value);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-color
			?showLabels=${this._showLabels}
			.swatches=${this._swatches}
			.value=${this.value?.value ?? ''}
			@change=${this._onChange}></umb-input-color>`;
	}
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
