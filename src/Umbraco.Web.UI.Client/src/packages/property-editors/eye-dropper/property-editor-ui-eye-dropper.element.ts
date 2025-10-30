import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIColorPickerChangeEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-eye-dropper
 */
@customElement('umb-property-editor-ui-eye-dropper')
export class UmbPropertyEditorUIEyeDropperElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean })
	mandatory = false;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

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

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-eye-dropper')!);
	}

	override render() {
		return html`
			<umb-input-eye-dropper
				.opacity=${this._opacity}
				.showPalette=${this._showPalette}
				.value=${this.value}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?readonly=${this.readonly}
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
