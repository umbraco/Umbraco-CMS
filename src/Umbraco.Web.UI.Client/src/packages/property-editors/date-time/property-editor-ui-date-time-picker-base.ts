import type { UmbTimeZonePickerValue } from '../time-zone-picker/property-editor-ui-time-zone-picker.element.js';
import type { InputDateType, UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import { css, html, nothing, property, repeat, state, until } from '@umbraco-cms/backoffice/external/lit';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import {
	getClientTimeZone,
	getTimeZoneList,
	getTimeZoneOffset,
	isEquivalentTimeZone,
	type UmbTimeZone,
} from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';

interface UmbDateTime {
	date: string | null;
	timeZone: string | null;
}

interface UmbTimeZonePickerOption extends UmbTimeZone {
	offset: string;
	invalid: boolean;
}

export abstract class UmbPropertyEditorUiDateTimePickerElementBase
	extends UmbFormControlMixin<UmbDateTime, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	private _timeZoneOptions: Array<UmbTimeZonePickerOption> = [];
	private _clientTimeZone: UmbTimeZone | undefined;
	private _timeZoneMode: UmbTimeZonePickerValue['mode'] | undefined;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage?: string | undefined;

	@state()
	protected _dateInputType: InputDateType = 'datetime-local';

	@state()
	protected _dateInputFormat: string = 'yyyy-MM-dd HH:mm:ss';

	@state()
	private _dateInputStep: number = 1;

	@state()
	private _selectedDate?: DateTime;

	@state()
	private _datePickerValue: string = '';

	@state()
	private _filteredTimeZoneOptions: Array<UmbTimeZonePickerOption> = [];

	@state()
	protected _displayTimeZone: boolean = true;

	@state()
	private _selectedTimeZone: string | undefined;

	constructor(dateInputType: InputDateType, displayTimeZone: boolean) {
		super();

		this._dateInputType = dateInputType;
		switch (dateInputType) {
			case 'date':
				this._dateInputFormat = 'yyyy-MM-dd';
				break;
			case 'time':
				this._dateInputFormat = 'HH:mm:ss';
				break;
		}
		this._displayTimeZone = displayTimeZone;

		this.addValidator(
			'customError',
			() => this.localize.term('dateTimePicker_emptyDate'),
			() => {
				return !!this.mandatory && !this.value?.date;
			},
		);

		this.addValidator(
			'customError',
			() => this.localize.term('dateTimePicker_emptyTimeZone'),
			() => {
				return !!this.value?.date && this._displayTimeZone && !this.value.timeZone;
			},
		);

		this.addValidator(
			'customError',
			() => this.localize.term('dateTimePicker_invalidTimeZone'),
			() => {
				return (
					this._displayTimeZone &&
					this._timeZoneMode !== 'local' &&
					!!this.value?.timeZone &&
					!this._timeZoneOptions.some((opt) => opt.value === this.value?.timeZone && !opt.invalid)
				);
			},
		);
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const timeFormat = config.getValueByAlias<string>('timeFormat');
		let timeZonePickerConfig: UmbTimeZonePickerValue | undefined = undefined;

		if (this._displayTimeZone) {
			timeZonePickerConfig = config.getValueByAlias<UmbTimeZonePickerValue>('timeZones');
		}

		this.#setTimeInputStep(timeFormat);
		this.#prefillValue(timeZonePickerConfig);

		// To ensure the expected value matches the prefilled value, we trigger an update.
		// If the values match, no change event will be fired.
		this.#updateValue(this._selectedDate?.toISO({ includeOffset: false }) || '');
	}

	#prefillValue(timeZonePickerConfig: UmbTimeZonePickerValue | undefined) {
		const date = this.value?.date;
		const zone = this.value?.timeZone;

		if (!date) {
			if (timeZonePickerConfig) {
				// If the date is not provided, we prefill the time zones using the current date (used to retrieve the offsets)
				this.#prefillTimeZones(timeZonePickerConfig, DateTime.now());
			}
			return;
		}

		// Load the date from the value
		const dateTime = DateTime.fromISO(date, { zone: zone ?? 'UTC' }); // If no zone is provided, we default to UTC.
		if (!dateTime.isValid) {
			if (timeZonePickerConfig) {
				// If the date is invalid, we prefill the time zones using the current date (used to retrieve the offsets)
				this.#prefillTimeZones(timeZonePickerConfig, DateTime.now());
			}
			console.warn(`[UmbPropertyEditorUIDateTimePickerElement] Invalid date format: ${date}`);
			return;
		}

		this._selectedDate = dateTime;
		this._datePickerValue = dateTime.toFormat(this._dateInputFormat);

		if (timeZonePickerConfig) {
			this.#prefillTimeZones(timeZonePickerConfig, dateTime);
		}
	}

	#prefillTimeZones(config: UmbTimeZonePickerValue | undefined, selectedDate: DateTime | undefined) {
		// Retrieve the time zones from the config
		this._clientTimeZone = getClientTimeZone();
		this._timeZoneMode = config?.mode;

		const dateToCalculateOffset = selectedDate ?? DateTime.now();
		switch (config?.mode) {
			case 'all':
				this._timeZoneOptions = this._filteredTimeZoneOptions = getTimeZoneList(undefined).map((tz) => ({
					...tz,
					offset: getTimeZoneOffset(tz.value, dateToCalculateOffset),
					invalid: false,
				}));
				break;
			case 'local': {
				this._timeZoneOptions = this._filteredTimeZoneOptions = [this._clientTimeZone].map((tz) => ({
					...tz,
					offset: getTimeZoneOffset(tz.value, dateToCalculateOffset),
					invalid: false,
				}));
				break;
			}
			case 'custom': {
				this._timeZoneOptions = this._filteredTimeZoneOptions = getTimeZoneList(config.timeZones).map((tz) => ({
					...tz,
					offset: getTimeZoneOffset(tz.value, dateToCalculateOffset),
					invalid: false,
				}));
				const selectedTimeZone = this.value?.timeZone;
				if (
					selectedTimeZone &&
					!this._timeZoneOptions.some((opt) => isEquivalentTimeZone(opt.value, selectedTimeZone))
				) {
					// If the selected time zone is not in the list, we add it to the options
					const customTimeZone: UmbTimeZonePickerOption = {
						value: selectedTimeZone,
						name: selectedTimeZone,
						offset: getTimeZoneOffset(selectedTimeZone, dateToCalculateOffset),
						invalid: true, // Mark as invalid, as it is not in the list of supported time zones
					};
					this._timeZoneOptions.push(customTimeZone);
				}
				break;
			}
			default:
				return;
		}

		this.#preselectTimeZone();
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
				this._selectedTimeZone = pickedTimeZone.value;
				return;
			}
		}

		// Check if we can pre-select the client time zone
		const clientTimeZone = this._clientTimeZone;
		const clientTimeZoneOpt =
			clientTimeZone &&
			this._timeZoneOptions.find(
				// A time zone name can be different in different browsers, so we need extra logic to match the client name with the options
				(option) => isEquivalentTimeZone(option.value, clientTimeZone.value),
			);
		if (clientTimeZoneOpt) {
			this._selectedTimeZone = clientTimeZoneOpt.value;
			if (this._selectedDate) {
				this._selectedDate = this._selectedDate.setZone(clientTimeZone.value);
				this._datePickerValue = this._selectedDate.toFormat(this._dateInputFormat);
			}
			return;
		}

		// If no time zone was selected still, we can default to the first option
		const firstOption = this._timeZoneOptions[0]?.value;
		this._selectedTimeZone = firstOption;
		if (this._selectedDate) {
			this._selectedDate = this._selectedDate.setZone(firstOption);
			this._datePickerValue = this._selectedDate.toFormat(this._dateInputFormat);
		}
	}

	#setTimeInputStep(timeFormat: string | undefined) {
		switch (timeFormat) {
			case 'HH:mm':
				this._dateInputStep = 60; // 1 hour
				break;
			case 'HH:mm:ss':
				this._dateInputStep = 1; // 1 second
				break;
			default:
				this._dateInputStep = 1;
				break;
		}
	}

	#onValueChange(event: CustomEvent & { target: UmbInputDateElement }) {
		const value = event.target.value.toString();
		const newPickerValue = value.replace('T', ' ');
		if (newPickerValue === this._datePickerValue) {
			return;
		}

		if (!newPickerValue) {
			this._datePickerValue = '';
			this.value = undefined;
			this._selectedDate = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		this._datePickerValue = newPickerValue;
		this.#updateValue(value, true);
	}

	#onTimeZoneChange(event: UUIComboboxEvent) {
		const timeZoneValue = (event.target as UUIComboboxElement).value.toString();
		if (timeZoneValue === this._selectedTimeZone) {
			return; // No change in time zone selection
		}

		this._selectedTimeZone = timeZoneValue;

		if (!this._selectedTimeZone) {
			if (this.value?.date) {
				this.value = { date: this.value.date, timeZone: null };
			} else {
				this.value = undefined;
			}
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		if (!this._selectedDate) {
			return;
		}

		this.#updateValue(this._selectedDate.toISO({ includeOffset: false }) || '');
	}

	#updateValue(date: string, updateOffsets = false) {
		// Try to parse the date with the selected time zone
		const newDate = DateTime.fromISO(date, { zone: this._selectedTimeZone ?? 'UTC' });

		// If the date is invalid, we reset the value
		if (!newDate.isValid) {
			if (!this.value) {
				return; // No change
			}
			this.value = undefined;
			this._selectedDate = undefined;
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		this._selectedDate = newDate;

		const timeZoneToStore = this._selectedTimeZone && this._timeZoneMode !== 'local' ? this._selectedTimeZone : 'UTC';
		const dateToStore = this._selectedTimeZone !== timeZoneToStore ? newDate.setZone(timeZoneToStore) : newDate;

		const newValue = {
			date: this.#formatDateValue(dateToStore),
			timeZone: this._timeZoneMode ? timeZoneToStore : null,
		};

		// Only update the stored data if it has actually changed to avoid firing unnecessary change events
		const previousValue = this.value;
		if (previousValue?.date === newValue.date && previousValue?.timeZone === newValue.timeZone) {
			return;
		}

		this.value = newValue;
		this.dispatchEvent(new UmbChangeEvent());

		if (updateOffsets) {
			this._timeZoneOptions.forEach((opt) => {
				opt.offset = getTimeZoneOffset(opt.value, newDate);
			});
			// Update the time zone options (mostly for the offset)
			this._filteredTimeZoneOptions = this._timeZoneOptions;
		}
	}

	#formatDateValue(date: DateTime): string | null {
		let formattedDate: string | undefined;
		switch (this._dateInputType) {
			case 'date':
				formattedDate = date.toFormat('yyyy-MM-dd');
				break;
			case 'time':
				formattedDate = date.toFormat('HH:mm:ss');
				break;
			default:
				formattedDate = date.toFormat(`yyyy-MM-dd'T'HH:mm:ss${this._timeZoneMode ? 'ZZ' : ''}`);
				break;
		}

		return formattedDate ?? null;
	}

	#onTimeZoneSearch(event: UUIComboboxEvent) {
		const searchTerm = (event.target as UUIComboboxElement)?.search;
		this._filteredTimeZoneOptions = this._timeZoneOptions.filter(
			(option) => option.name.toLowerCase().includes(searchTerm.toLowerCase()) || option.offset === searchTerm,
		);
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
		if (!this._displayTimeZone || this._timeZoneOptions.length === 0) {
			return nothing;
		}

		if (this._timeZoneOptions.length === 1) {
			return html`<span>${this._timeZoneOptions[0].name}</span> ${this._timeZoneOptions[0].value ===
				this._clientTimeZone?.value
					? ` (${this.localize.term('dateTimePicker_local')})`
					: nothing}`;
		}

		return html`
			<uui-combobox
				pristine
				@search="${this.#onTimeZoneSearch}"
				@change="${this.#onTimeZoneChange}"
				value=${this._selectedTimeZone || ''}
				?readonly=${this.readonly || this._timeZoneOptions.length === 1}>
				<uui-combobox-list>
					${until(repeat(this._filteredTimeZoneOptions, this.#renderTimeZoneOption))}
				</uui-combobox-list>
			</uui-combobox>
		`;
	}

	#renderTimeZoneOption = (option: UmbTimeZonePickerOption) =>
		html`<uui-combobox-list-option
			.value=${option.value}
			.displayValue=${option.name + (option.invalid ? ` (${this.localize.term('validation_legacyOption')})` : '')}>
			${option.name + (option.invalid ? ` (${this.localize.term('validation_legacyOption')})` : '')}
		</uui-combobox-list-option>`;

	#renderTimeZoneInfo() {
		if (
			this._timeZoneOptions.length === 0 ||
			!this._selectedTimeZone ||
			!this._selectedDate ||
			this._selectedTimeZone === this._clientTimeZone?.value
		) {
			return nothing;
		}

		return html` <span class="info"
			>${this.localize.term(
				'dateTimePicker_differentTimeZoneLabel',
				`UTC${this._selectedDate.toFormat('Z')}`,
				this._selectedDate.toLocal().toFormat('ff'),
			)}</span
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
			.error {
				color: var(--uui-color-invalid);
				font-size: var(--uui-font-size-small);
			}
		`,
	];
}
