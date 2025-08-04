import { getClientTimeZone, getTimeZoneList, isEquivalentTimeZone, type TimeZone } from '@umbraco-cms/backoffice/utils';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import {
	css,
	html,
	customElement,
	property,
	state,
	nothing,
	repeat,
	until,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { InputDateType, UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
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

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _dateFormat: string = 'date-time';

	@state()
	private _dateInputType: InputDateType = 'datetime-local';

	@state()
	private _dateInputFormat: string = 'yyyy-MM-dd HH:mm:ss';

	@state()
	private _dateInputStep: number = 1;

	@state()
	private _currentDate?: DateTime;

	@state()
	private _datePickerValue: string = '';

	@state()
	private _timeZoneOptions: Array<Option> = [];

	@state()
	private _filteredTimeZoneOptions: Array<Option> = [];

	@state()
	private _clientTimeZone: TimeZone = getClientTimeZone();

	@state()
	private _selectedTimeZone: string | undefined;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._dateFormat = config.getValueByAlias<string>('format') ?? 'date-time';
		const timeFormat = config.getValueByAlias<string>('timeFormat') ?? 'HH:mm';
		switch (this._dateFormat) {
			case 'date-only':
				this._dateInputType = 'date';
				this._dateInputFormat = 'yyyy-MM-dd';
				break;
			case 'time-only':
				this._dateInputType = 'time';
				this._dateInputFormat = 'HH:mm:ss';
				this.#setDateInputStep(timeFormat);
				break;
			default:
				this._dateInputType = 'datetime-local';
				this.#setDateInputStep(timeFormat);
				this.#prefillTimeZones(config);
				break;
		}

		this.#prefillValue();
	}

	#prefillValue() {
		const value = this._value;
		const zone = this._selectedTimeZone;
		if (!value?.date) {
			return;
		}

		const dateTime = DateTime.fromISO(value.date, { zone: zone ?? 'UTC' });
		if (!dateTime.isValid) {
			console.warn(`[UmbDateWithTimeZonePicker] Invalid date format: ${value.date}`);
			return;
		}

		this._currentDate = dateTime;
		this._datePickerValue = dateTime.toFormat(this._dateInputFormat);
	}

	#prefillTimeZones(config: UmbPropertyEditorConfigCollection) {
		// Retrieve the time zones from the config
		const timeZonePickerConfig = config.getValueByAlias<UmbTimeZonePickerValue>('timeZones');

		// Retrieve the time zones from the config
		switch (timeZonePickerConfig?.mode) {
			case 'all':
				this._timeZoneOptions = this._filteredTimeZoneOptions = getTimeZoneList().map((tz) => this.#mapTimeZoneOption(tz, false));
				this.#preselectTimeZone();
				break;
			case 'local': {
				const clientTimeZone = getClientTimeZone();
				this._timeZoneOptions = this._filteredTimeZoneOptions = [this.#mapTimeZoneOption(clientTimeZone, true)];
				this._selectedTimeZone = clientTimeZone.value;
				return;
			}
			case 'custom':
				this._timeZoneOptions = this._filteredTimeZoneOptions = getTimeZoneList(timeZonePickerConfig.timeZones).map(
					(tz) => this.#mapTimeZoneOption(tz, false),
				);
				this.#preselectTimeZone();
				break;
			default:
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

	#setDateInputStep(timeFormat: string) {
		switch (timeFormat) {
			case 'HH:mm':
				this._dateInputStep = 60; // 1 hour
				break;
			case 'HH:mm:ss':
				this._dateInputStep = 1; // 1 second
				break;
		}
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
		this.#updateValue(value);
		console.log(`[UmbDateWithTimeZonePicker] Value changed: ${value}`);
	}

	#onTimeZoneChange(event: UUIComboboxEvent) {
		const timeZoneValue = (event.target as UUIComboboxElement).value.toString();
		this._selectedTimeZone = timeZoneValue;

		if (!this._selectedTimeZone) {
			this._value = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		if (!this._currentDate) {
			return;
		}

		this.#updateValue(this._currentDate.toISO({ includeOffset: false }) || '');
		console.log(`[UmbDateWithTimeZonePicker] TimeZone changed: ${timeZoneValue}`);
	}

	#updateValue(date: string) {
		// Try to parse the date with the selected time zone
		const newDate = DateTime.fromISO(date, { zone: this._selectedTimeZone ?? 'UTC' });

		// If the date is invalid, we reset the value
		if (!newDate.isValid) {
			this._value = undefined;
			this._currentDate = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		this._currentDate = newDate;
		this._value = {
			date: this.#getCurrentDateValue(),
			timeZone: this._selectedTimeZone,
		};
		this.dispatchEvent(new UmbChangeEvent());
	}

	#getCurrentDateValue(): string | undefined {
		switch (this._dateFormat) {
			case 'date-only':
				return this._currentDate?.toFormat('yyyy-MM-dd');
			case 'time-only':
				return this._currentDate?.toFormat('HH:mm:ss');
			default:
				return this._currentDate?.toISO({ includeOffset: !!this._selectedTimeZone }) ?? undefined;
		}
	}

	override render() {
		return html`
			<div class="picker">
				<umb-input-date
					label=${this.localize.term('placeholders_enterdate')}
					.value=${this._datePickerValue}
					.step=${this._dateInputStep}
					type=${this._dateInputType}
					@change=${this.#onValueChange}
					?readonly=${this.readonly}>
				</umb-input-date>
				${this.#renderTimeZones()}
			</div>
			${this.#renderTimeZoneInfo()}
		`;
	}

	#renderTimeZones() {
		if (this._timeZoneOptions.length === 0) {
			return nothing;
		}

		if (this._timeZoneOptions.length === 1) {
			return html`<span>${this._timeZoneOptions[0].name}</span> ${this._timeZoneOptions[0].value ===
				this._clientTimeZone.value
					? ' (Local)'
					: nothing}`;
		}

		return html`
			<uui-combobox
				pristine
				search=""
				@search="${this.#onTimeZoneSearch}"
				@change="${this.#onTimeZoneChange}"
				value=${this._selectedTimeZone || ''}
				?readonly=${this.readonly || this._timeZoneOptions.length === 1}>
				<uui-combobox-list>
					${until(repeat(this._filteredTimeZoneOptions, this.#renderTimeZoneOption), html`Searching...`)}
				</uui-combobox-list>
			</uui-combobox>
		`;
	}

	#onTimeZoneSearch(event: UUIComboboxEvent) {
		const searchTerm = (event.target as UUIComboboxElement)?.search;
		this._filteredTimeZoneOptions = this._timeZoneOptions.filter((option) =>
			option.name.toLowerCase().includes(searchTerm),
		);
	}

	#renderTimeZoneOption = (option: Option) =>
		html`<uui-combobox-list-option .value=${option.value} .displayValue=${option.name}>
			${option.name}
		</uui-combobox-list-option>`;

	#renderTimeZoneInfo() {
		if (
			this._timeZoneOptions.length === 0 ||
			!this._selectedTimeZone ||
			!this._currentDate ||
			this._selectedTimeZone === this._clientTimeZone.value
		) {
			return nothing;
		}

		return html` <span class="info">
			The selected time (UTC${this._currentDate.toFormat('Z')}) is equivalent to
			${this._currentDate.toLocal().toFormat('ff')} in your local time.</span
		>`;
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
