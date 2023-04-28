import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUISliderEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-slider')
export class UmbInputSliderElement extends FormControlMixin(UmbLitElement) {
	

	@property({ type: Number })
	min = 0;

	@property({ type: Number })
	max = 100;

	@property({ type: Number })
	step = 1;

	@property({ type: Number })
	initVal1 = 0;

	@property({ type: Number })
	initVal2 = 0;

	@property({ type: Boolean, attribute: 'enable-range' })
	enableRange = false;

	protected getFormElement() {
		return undefined;
	}

	#onChange(e: UUISliderEvent) {
		e.stopPropagation();
		super.value = e.target.value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		if (this.enableRange) return this.#renderRangeSlider();
		else return this.#renderSlider();
	}

	#renderSlider() {
		return html`<uui-slider
			.min="${this.min}"
			.max="${this.max}"
			.step="${this.step}"
			.value="${this.initVal1.toString()}"
			@change="${this.#onChange}"></uui-slider>`;
	}
	#renderRangeSlider() {
		return html`<uui-range-slider
			.min="${this.min}"
			.max="${this.max}"
			.step="${this.step}"
			.valueLow="${this.initVal1}"
			.valueHigh="${this.initVal2}"
			@change="${this.#onChange}"></uui-range-slider>`;
	}
	
	static styles = [UUITextStyles, css``];
}

export default UmbInputSliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-slider': UmbInputSliderElement;
	}
}
