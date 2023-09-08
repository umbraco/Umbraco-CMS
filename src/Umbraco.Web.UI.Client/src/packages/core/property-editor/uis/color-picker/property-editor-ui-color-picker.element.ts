import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-color-picker
 */
@customElement('umb-property-editor-ui-color-picker')
export class UmbPropertyEditorUIColorPickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	#defaultShowLabels = false;

	@property()
	value = '';

	@state()
	private _showLabels = this.#defaultShowLabels;

	@state()
	private _swatches: UmbSwatchDetails[] = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._showLabels = config?.getValueByAlias('useLabel') ?? this.#defaultShowLabels;
		this._swatches = config?.getValueByAlias('items') ?? [];
	}

	private _onChange(event: UUIColorSwatchesEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-color
			@change="${this._onChange}"
			.swatches="${this._swatches}"
			.showLabels="${this._showLabels}"></umb-input-color>`;
	}
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
