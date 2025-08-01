import { getClientTimeZone, getTimeZoneList, isEquivalentTimeZone, type TimeZone } from '@umbraco-cms/backoffice/utils';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { InputDateType, UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbDateTimeWithTimeZone, UmbTimeZonePickerValue } from '@umbraco-cms/backoffice/models';
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
	private _format: 'date-only' | 'date-time' | 'date-time-timezone' = 'date-time-timezone';

	@state()
	private _dateInputType: InputDateType = 'datetime-local';

	@state()
	private _dateInputFormat: string = 'yyyy-MM-dd HH:mm';

	@state()
	private _currentDate?: DateTime;

	@state()
	private _datePickerValue: string = '';

	@state()
	private _timeZoneOffset: string = '';

	@state()
	private _timeZoneOptions: Array<Option> = [];

	@state()
	private _clientTimeZone: TimeZone = getClientTimeZone();

	@state()
	private _selectedTimeZone?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const format = config.getValueByAlias<string>('format') ?? 'date-time-timezone';
		switch (format) {
			case 'date-only':
				this._format = 'date-only';
				this._dateInputType = 'date';
				this._dateInputFormat = 'yyyy-MM-dd';
				break;
			case 'date-time':
				this._format = 'date-time';
				this._dateInputType = 'datetime-local';
				this._dateInputFormat = 'yyyy-MM-dd HH:mm';
				break;
			case 'date-time-timezone':
			default:
				this._format = 'date-time-timezone';
				this._dateInputType = 'datetime-local';
				this._dateInputFormat = 'yyyy-MM-dd HH:mm';
				break;
		}

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
		this._datePickerValue = dateTime.toFormat(this._dateInputFormat);
		this._timeZoneOffset = dateTime.toFormat('ZZ');
	}

	#prefillTimeZones(config: UmbPropertyEditorConfigCollection) {
		// Retrieve the time zones from the config
		const timeZonePickerConfig = config.getValueByAlias<UmbTimeZonePickerValue>('timeZones') || {
			mode: 'all',
			timeZones: [],
		};

		// Retrieve the time zones from the config
		switch (timeZonePickerConfig.mode) {
			case 'all':
				this._timeZoneOptions = getTimeZoneList().map((tz) => this.#mapTimeZoneOption(tz, false));
				this.#preselectTimeZone();
				break;
			case 'local':
				this._timeZoneOptions = [this.#mapTimeZoneOption(getClientTimeZone(), true)];
				this._selectedTimeZone = getClientTimeZone().value;
				this._timeZoneOffset = DateTime.now().toFormat('ZZ');
				return;
			case 'custom':
				this._timeZoneOptions = getTimeZoneList(timeZonePickerConfig.timeZones).map((tz) =>
					this.#mapTimeZoneOption(tz, false),
				);
				this.#preselectTimeZone();
				break;
			default:
				console.warn(`[UmbDateWithTimeZonePicker] Unknown time zone mode: ${timeZonePickerConfig.mode}`);
				return;
		}
	}

	#preselectTimeZone() {
		// Check whether there is a time zone in the value (stored previously)
		const selectedTimezone = this.value?.timeZone;
		if (selectedTimezone) {
			const pickedTimeZone = this._timeZoneOptions.find(
				// A time zone name can be different in different browsers, so we need extra logic to match the client name with the options
				(option) => isEquivalentTimeZone(option.value, selectedTimezone),
			);
			if (pickedTimeZone) {
				pickedTimeZone.selected = true;
				this._selectedTimeZone = pickedTimeZone.value;
				return;
			}
		}

		// Check if we can pre-select the client time zone
		const clientTimeZone = getClientTimeZone();
		const clientTimeZoneOpt = this._timeZoneOptions.find(
			// A time zone name can be different in different browsers, so we need extra logic to match the client name with the options
			(option) => isEquivalentTimeZone(option.value, clientTimeZone.value),
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

	#mapTimeZoneOption(timeZone: TimeZone, selected: boolean): Option {
		return {
			name: timeZone.name,
			value: timeZone.value,
			selected: selected,
		};
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
		if (!this._selectedTimeZone) {
			console.warn(`[UmbDateWithTimeZonePicker] No time zone selected, cannot update value.`);
			return;
		}

		this.#updateValue(value, this._selectedTimeZone);
		console.log(`[UmbDateWithTimeZonePicker] Value changed: ${value}`);
	}

	#onTimeZoneChange(event: UUISelectEvent) {
		const timeZoneValue = event.target.value.toString();
		this._selectedTimeZone = timeZoneValue;
		if (!this._currentDate) {
			return;
		}

		this.#updateValue(this._currentDate.toISO({ includeOffset: false }) || '', timeZoneValue);
		console.log(`[UmbDateWithTimeZonePicker] TimeZone changed: ${timeZoneValue}`);
	}

	#updateValue(date: string, timeZone: string) {
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
			timeZone: timeZone,
		};
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<div class="picker">
				<umb-input-date
					label=${this.localize.term('placeholders_enterdate')}
					.value=${this._datePickerValue}
					.step=${60}
					type=${this._dateInputType}
					@change=${this.#onValueChange}
					?readonly=${this.readonly}>
				</umb-input-date>
				${this.#renderTimeZones()}
			</div>
			${this._selectedTimeZone !== this._clientTimeZone.value && this._currentDate
				? html`<span class="info">
						The selected time (${this._currentDate.offsetNameShort}) is equivalent to
						${this._currentDate.toLocal().toFormat('ff')} in your local time.</span
					>`
				: nothing}
		`;
	}

	#renderTimeZones() {
		if (this._) {
			return html` <span>${this._timeZoneOptions[0].name} (Local)</span> `;
		}

		return html`
			<uui-select
				id="timeZone"
				name="timeZone"
				label="timeZone"
				.options=${this._timeZoneOptions || []}
				@change=${this.#onTimeZoneChange}
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
			.info {
				color: var(--uui-color-text-alt);
				font-size: var(--uui-type-small-size);
				font-weight: normal;
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
