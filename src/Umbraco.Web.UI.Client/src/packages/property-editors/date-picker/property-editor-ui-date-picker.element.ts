import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';

/**
 * This property editor allows the user to pick a date, datetime-local, or time.
 * It uses raw datetime strings back and forth, and therefore it has no knowledge
 * of timezones. It uses a regular HTML input element underneath, which supports the known
 * definitions of "date", "datetime-local", and "time".
 *
 * The underlying input element reports the value differently depending on the type configuration. Here
 * are some examples from the change event:
 *
 * date: 2024-05-03
 * datetime-local: 2024-05-03T10:44
 * time: 10:44
 *
 * These values are approximately similar to what Umbraco expects for the Umbraco.DateTime
 * data editor with one exception: the "T" character in "datetime-local". To be backwards compatible, we are
 * replacing the T character with a whitespace, which also happens to work just fine
 * with the "datetime-local" type.
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly: boolean = false;

	@state()
	private _inputType: UmbInputDateElement['type'] = 'datetime-local';

	@state()
	private _min?: string;

	@state()
	private _max?: string;

	@state()
	private _step?: number;

	@property()
	value?: string;

	@state()
	private _inputValue?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		// Format string prevalue/config
		const format = config.getValueByAlias<string>('format');
		const hasTime = (format?.includes('H') || format?.includes('m')) ?? false;
		const hasSeconds = format?.includes('s') ?? false;
		this._inputType = hasTime ? 'datetime-local' : 'date';

		// Based on the type of format string change the UUI-input type
		// Note: The format string is not validated, so it's possible to have an invalid format string,
		// but we do not use the format string for anything else than to determine the input type.
		// The format string is not used to validate the value and is only used on the frontend.
		const timeFormatPattern = /^h{1,2}:m{1,2}(:s{1,2})?\s?a?$/gim;
		if (format?.toLowerCase().match(timeFormatPattern)) {
			this._inputType = 'time';
		}

		this._min = config.getValueByAlias('min');
		this._max = config.getValueByAlias('max');
		this._step = (config.getValueByAlias('step') ?? hasSeconds) ? 1 : undefined;

		if (this.value) {
			this.#formatValue(this.value);
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputDateElement }) {
		let value = event.target.value.toString();

		if (!value) {
			this.#syncValue(undefined);
			return;
		}

		switch (this._inputType) {
			case 'time':
				value = `0001-01-01 ${value}`;
				break;
			case 'date':
				value = `${value} 00:00:00`;
				break;
			case 'datetime-local':
				value = value.replace('T', ' ');
				break;
		}

		this.#syncValue(value);
	}

	/**
	 * Formats the value depending on the input type.
	 * @param value
	 */
	#formatValue(value: string) {
		this._inputValue = undefined;

		if (isNaN(new Date(value).getTime())) {
			console.warn(`[UmbDatePicker] Invalid date: ${value}`);
			return;
		}

		const dateSplit = value.split(' ');
		if (dateSplit.length !== 2) {
			console.warn(`[UmbDatePicker] Invalid date: ${value}`);
			return;
		}

		switch (this._inputType) {
			case 'time':
				this._inputValue = dateSplit[1];
				break;
			case 'date':
				this._inputValue = dateSplit[0];
				break;
			default:
				this._inputValue = dateSplit.join('T');
				break;
		}
	}

	#syncValue(value?: string) {
		const valueHasChanged = this.value !== value;
		if (valueHasChanged) {
			this.value = value;
			this.dispatchEvent(new UmbPropertyValueChangeEvent());
		}
	}

	override render() {
		return html`
			<umb-input-date
				.value=${this._inputValue}
				.min=${this._min}
				.max=${this._max}
				.step=${this._step}
				.type=${this._inputType}
				@change=${this.#onChange}
				?readonly=${this.readonly}>
			</umb-input-date>
		`;
	}
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
