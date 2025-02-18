import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

export type InputDateType = 'date' | 'time' | 'datetime-local';

/**
 * This element passes a datetime string to a regular HTML input element.
 * Be aware that you cannot include a time demonination, i.e. "10:44:00" if you
 * set the input type of this element to "date". If you do, the browser will not show
 * the value at all.
 * @element umb-input-date
 */
@customElement('umb-input-date')
export class UmbInputDateElement extends UUIInputElement {
	/**
	 * Specifies the date and time type of the input that will be rendered.
	 * @type {InputDateType}
	 * @enum {InputDateType}
	 */
	override set type(type: InputDateType) {
		super.type = type;
	}
	override get type(): InputDateType {
		return super.type as InputDateType;
	}

	constructor() {
		super();
		this.type = 'date'; // Default to 'date'
	}
}

export default UmbInputDateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-date': UmbInputDateElement;
	}
}
