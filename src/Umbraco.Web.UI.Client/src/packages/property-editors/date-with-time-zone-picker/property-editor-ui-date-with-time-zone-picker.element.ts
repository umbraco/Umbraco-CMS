import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbDateTimeWithTimeZone } from '@umbraco-cms/backoffice/models';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';

/**
 * @element umb-property-editor-ui-date-with-time-zone-picker
 */
@customElement('umb-property-editor-ui-date-with-time-zone-picker')
export class UmbPropertyEditorUIDateWithTimeZonePickerElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@state()
	private _value: UmbDateTimeWithTimeZone | undefined;

	@property({ type: Object })
	public set value(value: UmbDateTimeWithTimeZone | undefined) {
		this._value = value;
	}

	public get value(): UmbDateTimeWithTimeZone | undefined {
		return this._value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _currentDate?: DateTime;

	@state()
	private _datePickerValue: string = '';

	@state()
	private _selectedTimeZone?: string = '';

	@state()
	private _timeZoneOffset: string = '';

	@state()
	private _timeZoneOptions: Array<Option> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this.#prefillValue();
		this.#prefillTimeZones(config);
	}

	#prefillValue() {
		const value = this._value;
		if (!value?.date) return;

		const dateTime = DateTime.fromISO(value.date, { zone: value.timeZone });
		if (!dateTime.isValid) {
			console.warn(`[UmbDateWithTimeZonePicker] Invalid date format: ${value.date}`);
			return;
		}

		this._currentDate = dateTime;
		this._datePickerValue = dateTime.toFormat('yyyy-MM-dd HH:mm');
		this._selectedTimeZone = value.timeZone;

		if (this._selectedTimeZone) {
			this._timeZoneOffset = dateTime.offsetNameShort || '';
		}
	}

	#prefillTimeZones(config: UmbPropertyEditorConfigCollection) {
		const timeZones = config.getValueByAlias<Array<string>>('timeZones') || [];
		this._timeZoneOptions = timeZones.map((tz) => this.#mapTimeZoneOption(tz));

		// If the picked timeZone is in the options, mark it as selected
		const selectedTimezone = this._selectedTimeZone;
		if (selectedTimezone) {
			const pickedTimeZone = this._timeZoneOptions.find(
				(option) => this.#getBrowserTimeZoneName(option.value) === this.#getBrowserTimeZoneName(selectedTimezone),
			);
			if (pickedTimeZone) {
				pickedTimeZone.selected = true;
				return;
			}
		}

		const clientTimeZone = this.#getClientTimeZone();

		// If the picked time zone was empty, check if we can select the client time zone
		const clientTimeZoneOpt = this._timeZoneOptions.find(
			// A time zone name can be different in different browsers, so we need extra logic to match the client name with the options
			(option) => this.#getBrowserTimeZoneName(option.value) === clientTimeZone,
		);
		if (clientTimeZoneOpt) {
			clientTimeZoneOpt.selected = true;
			this._selectedTimeZone = clientTimeZoneOpt.value;
			return;
		}

		// If no time zone was selected still, we can default to the first option
		const firstOption = this._timeZoneOptions[0];
		this._selectedTimeZone = firstOption.value;
		firstOption.selected = true;
	}

	#mapTimeZoneOption(timeZone: string, selected: boolean = false): Option {
		return {
			name: timeZone.replaceAll('_', ' '),
			value: timeZone,
			selected: selected,
		};
	}

	#getClientTimeZone() {
		return Intl.DateTimeFormat().resolvedOptions().timeZone;
	}

	#getBrowserTimeZoneName(timeZone: string): string {
		return new Intl.DateTimeFormat(undefined, { timeZone: timeZone }).resolvedOptions().timeZone;
	}

	#onValueChange(event: CustomEvent & { target: UmbInputDateElement }) {
		const value = event.target.value.toString();
		const newPickerValue = value.replace('T', ' ');
		if (newPickerValue === this._datePickerValue) {
			return;
		}

		if (!newPickerValue) {
			this._datePickerValue = '';
			this.#updateValue(undefined, this._selectedTimeZone);
			console.warn(`[UmbDateWithTimeZonePicker] Date picker value is empty.`);
			return;
		}

		this._datePickerValue = newPickerValue;
		this.#updateValue(value, this._selectedTimeZone);

		console.log(`[UmbDateWithTimeZonePicker] Value changed: ${value}`);
	}

	#onTimeZoneChange(event: UUISelectEvent) {
		const timeZoneValue = event.target.value.toString();
		if (timeZoneValue === this._selectedTimeZone) return;

		this._selectedTimeZone = timeZoneValue;
		this.#updateValue(this._datePickerValue, timeZoneValue);

		console.log(`[UmbDateWithTimeZonePicker] TimeZone changed: ${this._selectedTimeZone}`);
	}

	#updateValue(date: string | undefined, timeZone: string | undefined) {
		// If date or time zone are not set, we reset the value
		if (!date || !timeZone) {
			this._value = undefined;
			this._timeZoneOffset = '';
			this.dispatchEvent(new UmbChangeEvent());
			this._timeZoneOffset = DateTime.now().setZone(timeZone).offsetNameShort || '';
			return;
		}

		// Try to parse the date with the selected time zone
		const newDate = DateTime.fromFormat(this._datePickerValue, 'yyyy-MM-dd HH:mm', { zone: this._selectedTimeZone });

		// If the date is invalid, we reset the value
		if (!newDate.isValid) {
			this._value = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			this._timeZoneOffset = DateTime.now().setZone(timeZone).offsetNameShort || '';
			return;
		}

		this._currentDate = DateTime.fromISO(newDate.toISO(), { zone: timeZone });
		this._value = { date: this._currentDate.toISO() || '', timeZone: timeZone };
		this.dispatchEvent(new UmbChangeEvent());

		this._timeZoneOffset = this._currentDate.setZone(timeZone).offsetNameShort || '';
	}

	override render() {
		return html`
			<umb-input-date
				label=${this.localize.term('placeholders_enterdate')}
				.value=${this._datePickerValue}
				type="datetime-local"
				@change=${this.#onValueChange}
				?readonly=${this.readonly}>
			</umb-input-date>
			<uui-select
				id="timeZone"
				name="timeZone"
				label="timeZone"
				.options=${this._timeZoneOptions || []}
				@change=${this.#onTimeZoneChange}></uui-select>
			<span id="timeZoneOffset"> ${this._timeZoneOffset}</span>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-1);
			}
		`,
	];
}

export default UmbPropertyEditorUIDateWithTimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-with-time-zone-picker': UmbPropertyEditorUIDateWithTimeZonePickerElement;
	}
}
