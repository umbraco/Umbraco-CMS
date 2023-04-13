import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIInputEvent, UUISelectEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-date-picker')
export class UmbInputDatePickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
			}
		`,
	];

	protected getFormElement() {
		return undefined;
	}

	@property({ type: String })
	type = 'date';

	@property({ type: String })
	datetime = '';

	@property({ type: Boolean })
	enableTimezones = false;

	@state()
	private _currentTimezone?: string;

	constructor() {
		super();
	}

	options: Array<Option> = [
		{ name: 'Carrot', value: 'orange' },
		{ name: 'Cucumber', value: 'green' },
		{ name: 'Aubergine', value: 'purple' },
		{ name: 'Blueberry', value: 'Blue' },
		{ name: 'Banana', value: 'yellow' },
		{ name: 'Strawberry', value: 'red' },
	];

	connectedCallback(): void {
		super.connectedCallback();
		this.value = this.datetime;
	}

	#onDatetimeChange(e: UUIInputEvent) {
		e.stopPropagation();

		this.datetime = e.target.value as string;
		const append = this._currentTimezone ? `,${this._currentTimezone}` : '';
		this.value = this.datetime + append;

		this.dispatchEvent(new CustomEvent('change'));
	}

	#onTimezoneChange(e: UUISelectEvent) {
		e.stopPropagation();

		const tz = e.target.value as string;
		this._currentTimezone = tz;
		this.value = `${this.datetime},${tz}`;

		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`<uui-input
				id="datetime"
				@change="${this.#onDatetimeChange}"
				label="Pick a date or time"
				.type="${this.type}"
				.value="${this.datetime}"></uui-input>
			${this.enableTimezones ? this.#renderTimezone() : nothing}`;
	}

	#renderTimezone() {
		return html`<uui-select
			id="timezone"
			@change="${this.#onTimezoneChange}"
			label="Select timezone"
			placeholder="Pick timezone"
			.options="${this.options}"></uui-select>`;
	}
}

export default UmbInputDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-date-picker': UmbInputDatePickerElement;
	}
}
