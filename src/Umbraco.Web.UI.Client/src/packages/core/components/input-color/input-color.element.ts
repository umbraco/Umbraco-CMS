import { html, customElement, property, map, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';
import type { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/*
 * This wraps the UUI library uui-color-swatches component
 * @element umb-input-color
 */
@customElement('umb-input-color')
export class UmbInputColorElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement) {
	@property({ attribute: false })
	public override set value(v: string | undefined) {
		super.value = v;
	}
	public override get value(): string | undefined {
		return super.value as string | undefined;
	}
	protected override getFormElement() {
		return undefined;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean })
	required = false;
	@property({ type: String })
	requiredMessage: string = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ type: Boolean })
	showLabels = false;

	@property({ type: Array })
	swatches?: Array<UmbSwatchDetails>;

	constructor() {
		super();
		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !!this.required && !this.value && !this.readonly,
		);
	}

	#onChange(event: UUIColorSwatchesEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-color-swatches
				?readonly=${this.readonly}
				label="Color picker"
				value=${this.value ?? ''}
				@change=${this.#onChange}>
				${this.#renderColors()}
			</uui-color-swatches>
		`;
	}

	#renderColors() {
		if (!this.swatches) return nothing;
		return map(
			this.swatches,
			(swatch) => html`
				<uui-color-swatch label=${swatch.label} value=${swatch.value} .showLabel=${this.showLabels}></uui-color-swatch>
			`,
		);
	}
}

export default UmbInputColorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-color': UmbInputColorElement;
	}
}
