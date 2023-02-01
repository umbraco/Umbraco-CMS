import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbPropertyValueChangeEvent } from '../..';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	private updateValue(e: InputEvent) {
		console.log('config', this.config);

		const dateField = e.target as HTMLInputElement;
		this.value = dateField.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	@property({ type: Array, attribute: false })
	public config = [];

	// CONFIG 
	// Date format string
	// if empty = YYYY-MM-DD

	// Based on the format string
	// We need to change the underlying type for UUI-input

	// YYYY-MM-DD = date
	// YYYY-MM-DD HH:mm:ss = datetime-local
	// HH:mm:ss = time
	// HH:mm = time


	// Config offset time? (Boolean)
	// Copmpares against a global value in Umbraco.Sys.SeverVariables


	render() {
		return html`
			<uui-input type="date" @input=${this.updateValue} .value=${this.value}></uui-input>
			<div>
				<small>Chosen Value: ${this.value}</small>
			</div>`;
	}
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
