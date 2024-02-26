import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbMultipleColorPickerInputElement } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-color-swatches-editor
 */
@customElement('umb-property-editor-ui-color-swatches-editor')
export class UmbPropertyEditorUIColorSwatchesEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultShowLabels = true;

	@property({ type: Array })
	value: Array<UmbSwatchDetails> = [];

	@state()
	private _showLabels = this.#defaultShowLabels;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._showLabels = config?.getValueByAlias('useLabel') ?? this.#defaultShowLabels;
		const items = config?.getValueByAlias('items') as typeof this.value;
		if (items) {
			this.value = items;
		}
	}

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbMultipleColorPickerInputElement).items;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-multiple-color-picker-input
			?showLabels=${this._showLabels}
			.items="${this.value}"
			@change=${this.#onChange}></umb-multiple-color-picker-input>`;
	}
}

export default UmbPropertyEditorUIColorSwatchesEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-swatches-editor': UmbPropertyEditorUIColorSwatchesEditorElement;
	}
}
