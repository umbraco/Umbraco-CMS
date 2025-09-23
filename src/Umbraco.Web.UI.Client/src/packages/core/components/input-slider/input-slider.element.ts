import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUISliderEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbFormControlMixin } from '../../validation/mixins';

function stringToValueObject(value: string | undefined) {
	const [from, to] = (value ?? ',').split(',').map(Number);
	return { from, to: to ?? from };
}

function undefinedFallback(value: number | undefined, fallback: number) {
	return value === undefined ? fallback : value;
}

@customElement('umb-input-slider')
export class UmbInputSliderElement extends UmbFormControlMixin<string, typeof UmbLitElement, ''>(UmbLitElement, '') {
	@property()
	label: string = '';

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = 100;

	@property({ type: Number })
	step = 1;

	@property({ type: Number })
	public get valueLow(): number | undefined {
		return this.#valueLow;
	}
	public set valueLow(value: number | undefined) {
		this.#valueLow = value;
		this.#setValueFromLowHigh();
	}
	#valueLow?: number | undefined;

	@property({ type: Number })
	public get valueHigh(): number | undefined {
		return this.#valueHigh;
	}
	public set valueHigh(value: number | undefined) {
		this.#valueHigh = value;
		this.#setValueFromLowHigh();
	}
	#valueHigh?: number | undefined;

	#setValueFromLowHigh() {
		if (this.enableRange) {
			this.value = `${undefinedFallback(this.valueLow, this.min)},${undefinedFallback(this.valueHigh, this.max)}`;
		} else {
			this.value = `${undefinedFallback(this.valueLow, this.min)}`;
		}
	}

	@property({ type: Boolean, attribute: 'enable-range' })
	enableRange = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => {
				return this.localize.term('validation_numberMinimum', [this.min?.toString()]);
			},
			() => {
				const { from, to } = stringToValueObject(this.value);
				return this.min !== undefined ? from < this.min || to < this.min : false;
			},
		);

		this.addValidator(
			'rangeOverflow',
			() => {
				return this.localize.term('validation_numberMaximum', [this.max?.toString()]);
			},
			() => {
				const { from, to } = stringToValueObject(this.value);
				return this.max !== undefined ? from > this.max || to > this.max : false;
			},
		);

		this.addValidator(
			'patternMismatch',
			() => {
				return this.localize.term('validation_rangeExceeds');
			},
			() => {
				return this.min !== undefined && this.max !== undefined ? this.min > this.max : false;
			},
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: UUISliderEvent) {
		event.stopPropagation();
		const { from, to } = stringToValueObject(this.value);
		this.valueLow = from;
		this.valueHigh = to;
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return this.enableRange ? this.#renderRangeSlider() : this.#renderSlider();
	}

	#renderSlider() {
		return html`
			<uui-slider
				.label=${this.label}
				.min=${this.min}
				.max=${this.max}
				.step=${this.step}
				.value=${undefinedFallback(this.valueLow, this.min).toString()}
				@change=${this.#onChange}
				?readonly=${this.readonly}>
			</uui-slider>
		`;
	}

	#renderRangeSlider() {
		return html`
			<uui-range-slider
				.label=${this.label}
				.min=${this.min}
				.max=${this.max}
				.step=${this.step}
				.value="${undefinedFallback(this.valueLow, this.min).toString()},${undefinedFallback(
					this.valueHigh,
					this.max,
				).toString()}"
				@change=${this.#onChange}
				?readonly=${this.readonly}>
			</uui-range-slider>
		`;
	}
}

export default UmbInputSliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-slider': UmbInputSliderElement;
	}
}
