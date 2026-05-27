import { customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type InputDateType = 'date' | 'time' | 'datetime-local';

/**
 * This element passes a datetime string to a regular HTML input element.
 * Be aware that you cannot include a time denomination, i.e. "10:44:00" if you
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

		// On focusout, sync our value with whatever the native input shows now. This
		// captures an intentional clear that #onInput skipped while focus was inside.
		// Dispatch input and change so downstream listeners learn about the committed
		// value — the native change event that fires at blur was emitted before this
		// sync ran, and so carried the stale (pre-clear) value.
		this.addEventListener('focusout', () => {
			const native = this.shadowRoot?.querySelector('input');
			if (native && this.value !== native.value) {
				this.value = native.value;
				this.dispatchEvent(new UUIInputEvent(UUIInputEvent.INPUT));
				this.dispatchEvent(new UUIInputEvent(UUIInputEvent.CHANGE));
			}
		});
	}

	override onInput(e: Event) {
		e.stopPropagation();
		const target = e.target as HTMLInputElement;

		// The browser reports `.value === ''` while a date/datetime-local/time input is
		// in a transiently invalid state (e.g. a single "0" typed into a segment before
		// the second digit). Reflecting that empty value back via `this.value = ''` would
		// trigger a re-render that writes `''` to the native input, which clears all
		// segments visually and wipes the digit just typed. Skip the reflection here;
		// focusout commits the final value.
		if (target.value !== '') {
			this.value = target.value;
		}

		this.dispatchEvent(new UUIInputEvent(UUIInputEvent.INPUT));
	}

	// Adding styles override to add a darkmode version.
	static override styles = [
		...UUIInputElement.styles,
		css`
			input {
				color-scheme: var(--uui-color-scheme, normal);
			}
		`,
	];
}

export default UmbInputDateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-date': UmbInputDateElement;
	}
}
