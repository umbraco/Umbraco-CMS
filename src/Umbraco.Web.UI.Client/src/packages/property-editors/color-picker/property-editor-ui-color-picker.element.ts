import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-color-picker
 */
@customElement('umb-property-editor-ui-color-picker')
export class UmbPropertyEditorUIColorPickerElement
	extends UmbFormControlMixin<UmbSwatchDetails, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Object })
	public override set value(value: UmbSwatchDetails | undefined) {
		super.value = value ? this.#ensureHashPrefix(value) : undefined;
	}
	public override get value(): UmbSwatchDetails | undefined {
		return super.value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean }) mandatory = false;
	@property({ type: String }) mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	private _showLabels = false;

	@state()
	private _swatches: Array<UmbSwatchDetails> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._showLabels = config?.getValueByAlias<boolean>('useLabel') ?? false;

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
		this.dispatchEvent(new UmbChangeEvent());
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-color')!);
	}

	override render() {
		return html`<umb-input-color
			.value=${this.value?.value}
			.swatches=${this._swatches}
			?showLabels=${this._showLabels}
			@change=${this.#onChange}
			?required=${this.mandatory}
			.requiredMessage=${this.mandatoryMessage}
			?readonly=${this.readonly}></umb-input-color>`;
	}
}

export default UmbPropertyEditorUIColorPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-color-picker': UmbPropertyEditorUIColorPickerElement;
	}
}
