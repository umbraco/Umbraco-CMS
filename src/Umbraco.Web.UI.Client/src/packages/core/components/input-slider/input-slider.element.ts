import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUISliderEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-slider')
export class UmbInputSliderElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property()
	label: string = '';

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = 100;

	@property({ type: Number })
	step = 1;

	@property({ type: Number })
	valueLow = 0;

	@property({ type: Number })
	valueHigh = 0;

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
				.value=${this.valueLow.toString()}
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
				.value="${this.valueLow},${this.valueHigh}"
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
