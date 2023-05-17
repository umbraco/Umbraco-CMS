import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { InputType } from '@umbraco-ui/uui';
import { UmbPropertyValueChangeEvent } from '../..';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	private _value?: Date;
	private _valueString?: string;

	@property()
	set value(value: string | undefined) {
		if (value) {
			const d = new Date(value);
			this._value = d;
			this._valueString = `${d.getFullYear()}-${
				d.getMonth() + 1
			}-${d.getDate()}T${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`;
		} else {
			this._value = undefined;
			this._valueString = undefined;
		}
	}

	get value() {
		return this._valueString;
	}

	private _onInput(e: InputEvent) {
		const dateField = e.target as HTMLInputElement;
		this.value = dateField.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	private _format?: string;

	@state()
	private _inputType: InputType = 'datetime-local';

	@state()
	private _min?: string;

	@state()
	private _max?: string;

	@state()
	private _step?: number;

	private _offsetTime?: boolean;

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		const oldVal = this._inputType;

		// Format string prevalue/config
		this._format = config.getValueByAlias('format');
		const pickTime = this._format?.includes('H') || this._format?.includes('m');
		if (pickTime) {
			this._inputType = 'datetime-local';
		} else {
			this._inputType = 'date';
		}

		// Based on the type of format string change the UUI-input type
		const timeFormatPattern = /^h{1,2}:m{1,2}(:s{1,2})?\s?a?$/gim;
		if (this._format?.toLowerCase().match(timeFormatPattern)) {
			this._inputType = 'time';
		}

		//TODO:
		this._offsetTime = config.getValueByAlias('offsetTime');
		this._min = config.getValueByAlias('min');
		this._max = config.getValueByAlias('max');
		this._step = config.getValueByAlias('step');

		this.requestUpdate('_inputType', oldVal);
	}

	render() {
		return html`<umb-date-input
			.type=${this._inputType}
			@input=${this._onInput}
			.datetime=${this._valueString}
			.min=${this._min}
			.max=${this._max}
			.step=${this._step}
			.offsetTime=${this._offsetTime}
			label="Pick a date or time"></umb-date-input>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
