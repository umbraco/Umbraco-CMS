import { UmbFormControlMixin } from '../../validation/mixins/index.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUISliderEvent } from '@umbraco-cms/backoffice/external/uui';

function splitString(value: string | undefined): Partial<[number | undefined, number | undefined]> {
	const [from, to] = (value ?? ',').split(',');
	const fromNumber = makeNumberOrUndefined(from);
	return [fromNumber, makeNumberOrUndefined(to, fromNumber)];
}

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

function undefinedFallbackToString(value: number | undefined, fallback: number): string {
	return (value === undefined ? fallback : value).toString();
}

@customElement('umb-input-slider')
export class UmbInputSliderElement extends UmbFormControlMixin<string, typeof UmbLitElement, ''>(UmbLitElement, '') {
	override set value(value: string) {
		const [from, to] = splitString(value);
		this.#valueLow = from;
		this.#valueHigh = to;
		super.value = value;
	}
	override get value() {
		return super.value;
	}

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
			super.value = `${undefinedFallbackToString(this.valueLow, this.min)},${undefinedFallbackToString(this.valueHigh, this.max)}`;
		} else {
			super.value = `${undefinedFallbackToString(this.valueLow, this.min)}`;
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
				if (this.min !== undefined) {
					const [from, to] = splitString(this.value);
					if (to !== undefined && to < this.min) {
						return true;
					}
					if (from !== undefined && from < this.min) {
						return true;
					}
				}
				return false;
			},
		);

		this.addValidator(
			'rangeOverflow',
			() => {
				return this.localize.term('validation_numberMaximum', [this.max?.toString()]);
			},
			() => {
				if (this.max !== undefined) {
					const [from, to] = splitString(this.value);
					if (to !== undefined && to > this.max) {
						return true;
					}
					if (from !== undefined && from > this.max) {
						return true;
					}
				}
				return false;
			},
		);

		this.addValidator(
			'patternMismatch',
			() => {
				return this.localize.term('validation_rangeExceeds');
			},
			() => {
				const [from, to] = splitString(this.value);
				if (to !== undefined && from !== undefined) {
					return from > to;
				}
				return false;
			},
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	#onChange(event: UUISliderEvent) {
		event.stopPropagation();
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
				.value=${undefinedFallbackToString(this.valueLow, this.min).toString()}
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
				.value="${undefinedFallbackToString(this.valueLow, this.min).toString()},${undefinedFallbackToString(
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
