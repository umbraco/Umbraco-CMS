import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
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
	private _mode: 'local' | 'timezones' = 'timezones';

	@state()
	private _currentDate?: DateTime;

	@state()
	private _datePickerValue: string = '';

	@state()
	private _timeZoneOffset: string = '';

	@state()
	private _timeZoneOptions: Array<Option> = [];

	@state()
	private _selectedTimeZone?: string;

	@state()
	private _format: string = 'yyyy-MM-dd HH:mm:ss';

	@state()
	private _displayLocalTime: boolean = true;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this.#prefillTimeZones(config);
		this.#prefillValue();
	}

	#prefillValue() {
		const value = this._value;
		const zone = this._selectedTimeZone;
		if (!value?.date) {
			this._timeZoneOffset = DateTime.now().setZone(zone).toFormat('ZZ');
			return;
		}

		const dateTime = DateTime.fromISO(value.date, { zone: zone });
		if (!dateTime.isValid) {
			console.warn(`[UmbDateWithTimeZonePicker] Invalid date format: ${value.date}`);
			return;
		}

		this._currentDate = dateTime;
		this._datePickerValue = dateTime.toFormat(this._format);
		this._timeZoneOffset = dateTime.toFormat('ZZ');
	}

	#prefillTimeZones(config: UmbPropertyEditorConfigCollection) {
		this._displayLocalTime = config.getValueByAlias<boolean>('displayLocalTime') ?? false;

		// Retrieve the time zones from the config
		const timeZones = config.getValueByAlias<Array<string>>('timeZones') || [];

		// If no time zones are provided, we default to the client time zone
		const clientTimeZone = this.#getClientTimeZone();
		if(timeZones.length === 0){
			this._mode = 'local';
			this._timeZoneOptions = [this.#mapTimeZoneOption(clientTimeZone, true)];
			this._selectedTimeZone = clientTimeZone;
			this._timeZoneOffset = DateTime.now().toFormat('ZZ');
			return;
		}

		// If time zones are provided, we map them to options
		this._timeZoneOptions = timeZones.map((tz) => this.#mapTimeZoneOption(tz));

		// If there is already a time zone in the value (stored previously), we mark it as selected
		const selectedTimezone = this.value?.timeZone;
		if (selectedTimezone) {
			const pickedTimeZone = this._timeZoneOptions.find(
				// A time zone name can be different in different browsers, so we need extra logic to match the client name with the options
				(option) => this.#getBrowserTimeZoneName(option.value) === this.#getBrowserTimeZoneName(selectedTimezone),
			);
			if (pickedTimeZone) {
				pickedTimeZone.selected = true;
				this._selectedTimeZone = pickedTimeZone.value;
				return;
			}
		}

		// If the picked time zone was empty (or not found), check if we can pre-select the client time zone
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
		firstOption.selected = true;
		this._selectedTimeZone = firstOption.value;
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
			this._value = undefined;
			this._currentDate = undefined;
			this.dispatchEvent(new UmbChangeEvent());

			console.warn(`[UmbDateWithTimeZonePicker] Date picker value is empty.`);
			return;
		}

		this._datePickerValue = newPickerValue;
		if(!this._selectedTimeZone) {
			console.warn(`[UmbDateWithTimeZonePicker] No time zone selected, cannot update value.`);
			return;
		}

		this.#updateValue(value, this._selectedTimeZone);
		console.log(`[UmbDateWithTimeZonePicker] Value changed: ${value}`);
	}

	#onTimeZoneChange(event: UUISelectEvent) {
		const timeZoneValue = event.target.value.toString();
		this._selectedTimeZone = timeZoneValue;
		if(!this._currentDate){
			return;
		}

		this.#updateValue(this._currentDate.toISO({ includeOffset: false }) || '', timeZoneValue);
		console.log(`[UmbDateWithTimeZonePicker] TimeZone changed: ${timeZoneValue}`);
	}

	#updateValue(date : string, timeZone: string) {
		// Try to parse the date with the selected time zone
		const newDate = DateTime.fromISO(date, { zone: timeZone });

		// If the date is invalid, we reset the value
		if (!newDate.isValid) {
			this._value = undefined;
			this._currentDate = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		this._currentDate = newDate;
		this._value = {
				date: this._currentDate.toISO() || '',
				timeZone: timeZone };
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<div class="picker">
				<umb-input-date
					label=${this.localize.term('placeholders_enterdate')}
					.value=${this._datePickerValue}
					.step=${1}
					type="datetime-local"
					@change=${this.#onValueChange}
					?readonly=${this.readonly}>
				</umb-input-date>
				${this.#renderTimeZones()}
			</div>
			${this._currentDate && this._displayLocalTime ? html`<span><b>Local:</b> ${this._currentDate.toLocal().toFormat(this._format)}</span>` : nothing}
		`;
	}

	#renderTimeZones() {
		if(this._mode === 'local') {
			return html`
				<span title="UTC${this._timeZoneOffset}">${this._timeZoneOptions[0].name} (Local)</span>
				`;
		}

		return html`
			<uui-select
				id="timeZone"
				name="timeZone"
				label="timeZone"
				.options=${this._timeZoneOptions || []}
				@change=${this.#onTimeZoneChange}
				title="UTC${this._timeZoneOffset}"
				?readonly=${this.readonly || this._timeZoneOptions.length === 1}></uui-select>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
			.picker {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
			.info{
				display: flex;
				gap: var(--uui-size-space-3);
				padding-top: var(--uui-size-space-3);
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
