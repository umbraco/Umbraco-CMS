import type { UmbSliderPropertyEditorUiValue, UmbSliderPropertyEditorUiValueObject } from './types.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputSliderElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 *
 * @param value
 */
function stringToValueObject(value: string | undefined): Partial<UmbSliderPropertyEditorUiValueObject> {
	const [from, to] = (value ?? ',').split(',');
	const fromNumber = makeNumberOrUndefined(from);
	return { from: fromNumber, to: makeNumberOrUndefined(to, fromNumber) };
}

/**
 *
 * @param value
 * @param fallback
 */
function makeNumberOrUndefined(value: string | undefined, fallback?: undefined | number) {
	if (value === undefined) {
		return fallback;
	}
	const n = Number(value);
	if (isNaN(n)) {
		return fallback;
	}
	return n;
}

/**
 *
 * @param value
 * @param fallback
 */
function undefinedFallback(value: number | undefined, fallback: number) {
	return value === undefined ? fallback : value;
}

/**
 * @element umb-property-editor-ui-slider
 */
@customElement('umb-property-editor-ui-slider')
export class UmbPropertyEditorUISliderElement
	extends UmbFormControlMixin<UmbSliderPropertyEditorUiValue, typeof UmbLitElement>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	private _enableRange = false;

	@state()
	private _initVal1: number = 0;

	@state()
	private _initVal2: number = 1;

	@state()
	private _label?: string;

	@state()
	private _step = 1;

	@state()
	private _min = 0;

	@state()
	private _max = 100;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._enableRange = Boolean(config.getValueByAlias('enableRange'));

		// Make sure that step is higher than 0 (decimals ok).
		const step = (config.getValueByAlias('step') ?? 1) as number;
		this._step = step > 0 ? step : 1;

		const initVal1 = Number(config.getValueByAlias('initVal1'));
		this._initVal1 = isNaN(initVal1) ? 0 : initVal1;

		const initVal2 = Number(config.getValueByAlias('initVal2'));
		this._initVal2 = isNaN(initVal2) ? this._initVal1 + this._step : initVal2;

		this._min = this.#parseNumber(config.getValueByAlias('minVal')) || 0;
		this._max = this.#parseNumber(config.getValueByAlias('maxVal')) || 100;

		if (this._min === this._max) {
			this._max = this._min + 100;
			console.warn(
				`Property Editor (Slider) has been misconfigured, 'min' and 'max' are equal values. Please correct your data type configuration. To render the slider correctly, we changed this slider to: min = ${this._min}, max = ${this._max}`,
				this,
			);
		}
	}

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this._label = context?.getLabel();
		});
	}

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-slider')!);
		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property '${this._label}' (Slider) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#parseNumber(input: unknown): number | undefined {
		const num = Number(input);
		return Number.isFinite(num) ? num : undefined;
	}

	#onChange(event: CustomEvent & { target: UmbInputSliderElement }) {
		const partialValue = stringToValueObject(event.target.value as string);
		const handledFrom = undefinedFallback(partialValue.from, this._initVal1);
		this.value = {
			from: handledFrom,
			to: this._enableRange ? undefinedFallback(partialValue.to, this._initVal2) : handledFrom,
		};
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-slider
				.label=${this._label ?? 'Slider'}
				.valueLow=${undefinedFallback(this.value?.from, this._initVal1)}
				.valueHigh=${undefinedFallback(this.value?.to, this._initVal2)}
				.step=${this._step}
				.min=${this._min}
				.max=${this._max}
				?enable-range=${this._enableRange}
				@change=${this.#onChange}
				?readonly=${this.readonly}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}>
			</umb-input-slider>
		`;
	}
}

export default UmbPropertyEditorUISliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-slider': UmbPropertyEditorUISliderElement;
	}
}
