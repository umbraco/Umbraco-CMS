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
	public set value(value: UmbSwatchDetails | undefined) {
		if (!value) return;
		this.#value = this.#ensureHashPrefix(value);
	}
	public get value(): UmbSwatchDetails | undefined {
		return this.#value;
	}
	#value?: UmbSwatchDetails | undefined;

	@state()
	private _showLabels = this.#defaultShowLabels;

	@state()
	private _swatches: Array<UmbSwatchDetails> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._showLabels = config?.getValueByAlias<boolean>('useLabel') ?? this.#defaultShowLabels;

		const swatches = config?.getValueByAlias<Array<UmbSwatchDetails>>('items') ?? [];
		this._swatches = swatches.map((swatch) => this.#ensureHashPrefix(swatch));
	}

	#ensureHashPrefix(swatch: UmbSwatchDetails): UmbSwatchDetails {
		return {
			label: swatch.label,
			// hex color regex adapted from: https://stackoverflow.com/a/9682781/12787
			value: swatch.value.match(/^(?:[0-9a-f]{3}){1,2}$/i) ? `#${swatch.value}` : swatch.value,
		};
	}

	#onChange(event: UUIColorSwatchesEvent) {
		const value = event.target.value;
		this.value = this._swatches.find((swatch) => swatch.value === value);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<umb-input-color
			value=${this.value?.value ?? ''}
			.swatches=${this._swatches}
			?showLabels=${this._showLabels}
			@change=${this.#onChange}></umb-input-color>`;
	}
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
