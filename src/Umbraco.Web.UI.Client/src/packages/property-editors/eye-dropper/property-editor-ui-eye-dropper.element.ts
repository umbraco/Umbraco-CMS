import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIColorPickerChangeEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-eye-dropper
 */
@customElement('umb-property-editor-ui-eye-dropper')
export class UmbPropertyEditorUIEyeDropperElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _opacity = false;

	@state()
	private _showPalette = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._opacity = config.getValueByAlias('showAlpha') ?? false;
		this._showPalette = config.getValueByAlias('showPalette') ?? false;
	}

	#onChange(event: UUIColorPickerChangeEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-eye-dropper
				.opacity=${this._opacity}
				.showPalette=${this._showPalette}
				value=${this.value}
				@change=${this.#onChange}></umb-input-eye-dropper>
		`;
	}
}

export default UmbPropertyEditorUIEyeDropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-eye-dropper': UmbPropertyEditorUIEyeDropperElement;
	}
}
