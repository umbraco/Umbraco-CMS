import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from 'src/packages/core/property/property-editor';
import { UmbMultipleColorPickerInputElement } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-color-editor
 */
@customElement('umb-property-editor-ui-color-editor')
export class UmbPropertyEditorUIColorEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultShowLabels = true;

	@property({ type: Array })
	value: Array<UmbSwatchDetails> = [];

	@state()
	private _showLabels = this.#defaultShowLabels;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._showLabels = config?.getValueByAlias('useLabel') ?? this.#defaultShowLabels;
		this.value = config?.getValueByAlias('items') ?? [];
	}

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbMultipleColorPickerInputElement).items;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-multiple-color-picker-input
			?showLabels=${this._showLabels}
			.items="${this.value ?? []}"
			@change=${this.#onChange}></umb-multiple-color-picker-input>`;
	}
}

export default UmbPropertyEditorUIColorEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-editor': UmbPropertyEditorUIColorEditorElement;
	}
}
