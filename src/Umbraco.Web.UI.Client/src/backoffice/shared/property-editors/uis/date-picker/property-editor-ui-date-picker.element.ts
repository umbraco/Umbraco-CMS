import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbPropertyValueChangeEvent } from '../..';
import { UmbLitElement } from '@umbraco-cms/element';
import { PropertyEditorConfigDefaultData } from '@umbraco-cms/extensions-registry';


/**
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	private updateValue(e: InputEvent) {
		const dateField = e.target as HTMLInputElement;
		this.value = dateField.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	private _format?: string;

	// TODO: Warren [Can I get the underlying UUI-input type/enum so I can ensure only valid input types set]
	private _inputType?: string = "date";

	private _offsetTime?: boolean;

	@property({ type: Array, attribute: false })
	public set config(config: Array<PropertyEditorConfigDefaultData>) {

		console.log('config', config);

		// Format string prevalue/config
		this._format = config.find((x) => x.alias === 'format')?.value;

		// TODO: Warren 
		// When this is set to true then you need to check the Umbraco.Sys.ServerVariables that comes from C# server in a global JS obejct
		this._offsetTime = config.find((x) => x.alias === 'offsetTime')?.value;

		// Based on the type of format string change the UUI-input type
		switch (this._format) {
			case 'YYYY-MM-DD':
				this._inputType = "date";
				break;

			case 'YYYY-MM-DD HH:mm:ss':
				this._inputType = "datetime-local";
				break;

			case 'HH:mm:ss':
				this._inputType = "time";
				break;
			
			default:
				break;
		}
	}

	render() {
		return html`
			<uui-input .type=${this._inputType} @input=${this.updateValue} .value=${this.value}></uui-input>
			<div>
				<small>Chosen Value: ${this.value}</small><br/>
				<small>Config Format: ${this._format}</small><br/>
				<small>Config TimeOffset enabled?: ${this._offsetTime}</small><br/>
			</div>`;
	}
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
