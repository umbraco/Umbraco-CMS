import { UmbConfigRepository } from '../../repositories/config/config.repository.js';
import { css, html, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles, FormControlMixin, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-date')
export class UmbInputDateElement extends FormControlMixin(UmbLitElement) {
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

	@property({ type: Boolean })
	offsetTime = false;

	@state()
	private _offsetValue = 0;

	@property({ type: String })
	min?: string;

	@property({ type: String })
	max?: string;

	@property({ type: Number })
	step?: number;

	private _configRepository = new UmbConfigRepository(this);

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.offsetTime ? this.#getOffset() : (this.displayValue = this.#UTCToLocal(this.value as string));
	}

	async #getOffset() {
		const data = await this._configRepository.getServertimeOffset();
		if (!data) return;
		this._offsetValue = data.offset;

		if (!this.value) return;
		this.displayValue = this.#valueToServerOffset(this.value as string, true);
	}

	#localToUTC(d: string) {
		if (this.type === 'time') {
			return new Date(`${new Date().toJSON().slice(0, 10)} ${d}`).toISOString().slice(11, 16);
		} else {
			const date = new Date(d);
			const isoDate = date.toISOString();
			return `${isoDate.substring(0, 10)}T${isoDate.substring(11, 19)}Z`;
		}
	}

	#UTCToLocal(d: string) {
		if (this.type === 'time') {
			const local = new Date(`${new Date().toJSON().slice(0, 10)} ${d}Z`)
				.toLocaleTimeString(undefined, {
					hourCycle: 'h23',
				})
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

	#dateToString(date: Date) {
		return `${date.getFullYear()}-${('0' + (date.getMonth() + 1)).slice(-2)}-${('0' + date.getDate()).slice(-2)}T${(
			'0' + date.getHours()
		).slice(-2)}:${('0' + date.getMinutes()).slice(-2)}:${('0' + date.getSeconds()).slice(-2)}`;
	}

	#valueToServerOffset(d: string, utc = false) {
		if (this.type === 'time') {
			const newDate = new Date(`${new Date().toJSON().slice(0, 10)} ${d}`);
			const dateOffset = new Date(
				newDate.setTime(newDate.getTime() + (utc ? this._offsetValue * -1 : this._offsetValue) * 60 * 1000)
			);
			const time = dateOffset
				.toLocaleTimeString(undefined, {
					hourCycle: 'h23',
				})
				.slice(0, 5);
			return time;
		} else {
			const newDate = new Date(d.replace('Z', ''));
			const dateOffset = new Date(
				newDate.setTime(newDate.getTime() + (utc ? this._offsetValue * -1 : this._offsetValue) * 60 * 1000)
			);
			return this.type === 'datetime-local'
				? this.#dateToString(dateOffset)
				: this.#dateToString(dateOffset).slice(0, 10);
		}
	}

	#onChange(e: UUIInputEvent) {
		e.stopPropagation();
		const picked = e.target.value as string;
		if (!picked) {
			this.value = '';
			this.displayValue = '';
			return;
		}
		this.value = this.offsetTime ? this.#valueToServerOffset(picked) : this.#localToUTC(picked);
		this.displayValue = picked;
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`<uui-input
			id="datetime"
			label="Pick a date or time"
			.type="${this.type}"
			@change="${this.#onChange}"
			min="${ifDefined(this.min)}"
			max="${ifDefined(this.max)}"
			.step="${this.step}"
			.value="${this.displayValue?.replace('Z', '')}">
		</uui-input>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbInputDateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-date': UmbInputDateElement;
	}
}
