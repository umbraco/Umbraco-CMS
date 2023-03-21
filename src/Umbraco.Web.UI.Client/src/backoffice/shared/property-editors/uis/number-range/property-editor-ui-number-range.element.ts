import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import type { UmbInputNumberRangeElement } from '../../../../shared/components/input-number-range/input-number-range.element';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';
import { UmbLitElement } from '@umbraco-cms/element';
import '../../../../shared/components/input-number-range/input-number-range.element';

type ValueType = {
	min?: number;
	max?: number;
};

/**
 * @element umb-property-editor-ui-number-range
 */
@customElement('umb-property-editor-ui-number-range')
export class UmbPropertyEditorUINumberRangeElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property({ type: Object })
	private _value: ValueType = { min: undefined, max: undefined };
	public get value() {
		return this._value;
	}
	public set value(value: ValueType | undefined) {
		this._value = value || { min: undefined, max: undefined };
		this._minValue = value?.min;
		this._maxValue = value?.max;
	}

	@property({ type: Array, attribute: false })
	public config = [];

	private _onChange(event: CustomEvent) {
		this.value = {
			min: (event.target as UmbInputNumberRangeElement).minValue,
			max: (event.target as UmbInputNumberRangeElement).maxValue,
		};
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	@state()
	_minValue?: number;
	@state()
	_maxValue?: number;

	render() {
		return html`<umb-input-number-range
			.minValue=${this._minValue}
			.maxValue=${this._maxValue}
			@change=${this._onChange}></umb-input-number-range>`;
	}
}

export default UmbPropertyEditorUINumberRangeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-number-range': UmbPropertyEditorUINumberRangeElement;
	}
}
