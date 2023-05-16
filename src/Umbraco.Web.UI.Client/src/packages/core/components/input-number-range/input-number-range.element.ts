import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

function getNumberOrUndefined(value: string) {
	const num = parseInt(value, 10);
	return isNaN(num) ? undefined : num;
}

@customElement('umb-input-number-range')
export class UmbInputNumberRangeElement extends FormControlMixin(UmbLitElement) {
	

	@property({ type: String, attribute: 'min-label' })
	minLabel = 'Low value';

	@property({ type: String, attribute: 'max-label' })
	maxLabel = 'High value';

	@state()
	private _minValue?: number;
	@property()
	public get minValue() {
		return this._minValue;
	}
	public set minValue(value: number | undefined) {
		this._minValue = value;
		this.updateValue();
	}

	@state()
	private _maxValue?: number;
	@property()
	public get maxValue() {
		return this._maxValue;
	}
	public set maxValue(value: number | undefined) {
		this._maxValue = value;
		this.updateValue();
	}

	private updateValue() {
		const newValue = this._minValue || this._maxValue ? (this._minValue || '') + ',' + (this._maxValue || '') : '';
		if (super.value !== newValue) {
			super.value = newValue;
		}
	}

	@property()
	public set value(valueString: string) {
		if (valueString !== this._value) {
			const splittedValue = valueString.split(/[ ,]+/);
			this.minValue = getNumberOrUndefined(splittedValue[0]);
			this.maxValue = getNumberOrUndefined(splittedValue[1]);
		}
	}

	constructor() {
		super();
	}

	protected getFormElement() {
		return this;
	}

	private _onMinInput(e: InputEvent) {
		this.minValue = Number((e.target as HTMLInputElement).value);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	private _onMaxInput(e: InputEvent) {
		this.maxValue = Number((e.target as HTMLInputElement).value);
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html`<uui-input
				type="number"
				.value=${this._minValue}
				@input=${this._onMinInput}
				label=${this.minLabel}></uui-input>
			–
			<uui-input type="number" .value=${this._maxValue} @input=${this._onMaxInput} label=${this.maxLabel}></uui-input>`;
	}
	
	static styles = [UUITextStyles, css``];
}

export default UmbInputNumberRangeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-number-range': UmbInputNumberRangeElement;
	}
}
