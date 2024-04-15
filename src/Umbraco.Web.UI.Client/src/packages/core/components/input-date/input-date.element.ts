import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-date')
export class UmbInputDateElement extends UUIFormControlMixin(UmbLitElement, '') {
	protected getFormElement() {
		return undefined;
	}

	/**
	 * Specifies the type of input that will be rendered.
	 * @type {'date'| 'time'| 'datetime-local'}
	 * @attr
	 * @default date
	 */
	@property()
	type: 'date' | 'time' | 'datetime-local' = 'date';

	@property({ type: String })
	displayValue?: string;

	@property({ type: String })
	min?: string;

	@property({ type: String })
	max?: string;

	@property({ type: Number })
	step?: number;

	connectedCallback(): void {
		super.connectedCallback();

		if (!this.value) return;
		this.displayValue = this.#UTCToLocal(this.value as string);
	}

	#localToUTC(date: string) {
		if (this.type === 'time') {
			return new Date(`${new Date().toJSON().slice(0, 10)} ${date}`).toISOString().slice(11, 16);
		} else {
			return new Date(date).toJSON();
		}
	}

	#UTCToLocal(d: string) {
		if (this.type === 'time') {
			const local = new Date(`${new Date().toJSON().slice(0, 10)} ${d}Z`)
				.toLocaleTimeString(undefined, { hourCycle: 'h23' })
				.slice(0, 5);
			return local;
		} else {
			const timezoneReset = `${d.replace('Z', '')}Z`;
			const date = new Date(timezoneReset);

			const dateString = `${date.getFullYear()}-${('0' + (date.getMonth() + 1)).slice(-2)}-${(
				'0' + date.getDate()
			).slice(-2)}T${('0' + date.getHours()).slice(-2)}:${('0' + date.getMinutes()).slice(-2)}:${(
				'0' + date.getSeconds()
			).slice(-2)}`;

			return this.type === 'datetime-local' ? dateString : `${dateString.substring(0, 10)}`;
		}
	}

	#onChange(event: UUIInputEvent) {
		const newValue = event.target.value as string;
		if (!newValue) return;

		this.value = this.#localToUTC(newValue);
		this.displayValue = newValue;
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`<uui-input
			id="datetime"
			label="Pick a date or time"
			.min=${this.min}
			.max=${this.max}
			.step=${this.step}
			.type=${this.type}
			.value="${this.displayValue?.replace('Z', '')}"
			@change=${this.#onChange}>
		</uui-input>`;
	}
}

export default UmbInputDateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-date': UmbInputDateElement;
	}
}
