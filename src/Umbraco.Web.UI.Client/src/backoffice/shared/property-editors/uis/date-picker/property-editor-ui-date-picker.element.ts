import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { InputType } from '@umbraco-ui/uui';
import { UmbPropertyValueChangeEvent } from '../..';
import { UmbLitElement } from '@umbraco-cms/element';
import { PropertyEditorConfigDefaultData } from '@umbraco-cms/extensions-registry';


/**
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	private _value?: Date;
	private _valueString?: string;

	@property()
	set value(value: string | undefined) {
		if (value) {
			const d = new Date(value);
			this._value = d;
			this._valueString = `${d.getFullYear()}-${d.getMonth() + 1}-${d.getDate()}T${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`;
		} else {
			this._value = undefined;
			this._valueString = undefined;
		}
	}

	get value(): string | undefined {
		if (this._value) {
			const d = this._value;
			return `${d.getFullYear()}-${d.getMonth() + 1}-${d.getDate()} ${d.getHours()}:${d.getMinutes()}:${d.getSeconds()}`;
		}
		return undefined;
	}

	private updateValue(e: InputEvent) {
		const dateField = e.target as HTMLInputElement;
		this.value = dateField.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	private _format?: string;

	@state()
	private _inputType: InputType = "datetime-local";

	private _offsetTime?: boolean;

	@property({ type: Array, attribute: false })
	public set config(config: Array<PropertyEditorConfigDefaultData>) {

		console.log('config', config);

		// Format string prevalue/config
		this._format = config.find((x) => x.alias === 'format')?.value;
		const pickTime = this._format?.includes('H') || this._format?.includes('m');
		if (pickTime) {
			this._inputType = "datetime-local";
		} else {
			this._inputType = "date";
		}

		// Based on the type of format string change the UUI-input type
		const timeFormatPattern = /^h{1,2}:m{1,2}(:s{1,2})?\s?a?$/gmi;
        if (this._format?.toLowerCase().match(timeFormatPattern)) {
			this._inputType = "time";
		}

		// TODO: Warren - could this be handled in the C# server side code?
		// When this is set to true then you need to check the Umbraco.Sys.ServerVariables that comes from C# server in a global JS obejct
		// How do we handle this ?
		this._offsetTime = config.find((x) => x.alias === 'offsetTime')?.value;
	}

	render() {
		return html`
			<uui-input .type=${this._inputType} @input=${this.updateValue} .value=${this._valueString} label="Pick a date or time"></uui-input>`;
	}
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
