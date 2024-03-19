import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIColorPickerChangeEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-eye-dropper
 */
@customElement('umb-property-editor-ui-eye-dropper')
export class UmbPropertyEditorUIEyeDropperElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultOpacity = false;

	@property()
	value = '';

	@state()
	private _opacity = this.#defaultOpacity;

	@state()
	private _swatches: string[] = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (config) {
			this._opacity = config.getValueByAlias('showAlpha') ?? this.#defaultOpacity;
			this._swatches = config.getValueByAlias('palette') ?? [];
		}
	}

	#onChange(event: UUIColorPickerChangeEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-eye-dropper
			.opacity=${this._opacity}
			.swatches=${this._swatches}
			.value=${this.value}
			@change=${this.#onChange}></umb-input-eye-dropper>`;
	}
}

export default UmbPropertyEditorUIEyeDropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-eye-dropper': UmbPropertyEditorUIEyeDropperElement;
	}
}
