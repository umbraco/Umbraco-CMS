import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbMultipleColorPickerInputElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';

/**
 * @element umb-property-editor-ui-color-swatches-editor
 */
@customElement('umb-property-editor-ui-color-swatches-editor')
export class UmbPropertyEditorUIColorSwatchesEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultShowLabels = false;

	@property({ type: Array })
	value: Array<UmbSwatchDetails> = [];

	@state()
	private _showLabels = this.#defaultShowLabels;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._showLabels = config?.getValueByAlias('useLabel') ?? this.#defaultShowLabels;
	}

	#onChange(event: CustomEvent & { target: UmbMultipleColorPickerInputElement }) {
		this.value = event.target.items;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-multiple-color-picker-input
				.items=${this.value}
				?showLabels=${this._showLabels}
				@change=${this.#onChange}></umb-multiple-color-picker-input>
		`;
	}
}

export default UmbPropertyEditorUIColorSwatchesEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-swatches-editor': UmbPropertyEditorUIColorSwatchesEditorElement;
	}
}
