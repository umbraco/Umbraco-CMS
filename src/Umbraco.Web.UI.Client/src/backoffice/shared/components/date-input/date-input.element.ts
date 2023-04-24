import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-date-input')
export class UmbDateInputElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles, css``];

	protected getFormElement() {
		return undefined;
	}

	@property({ type: String })
	type = 'date';

	@property({ type: String })
	min?: string;

	@property({ type: String })
	max?: string;

	@property({ type: Number })
	step?: number;

	@property({ type: Boolean })
	offsetTime = false;

	@state()
	private _localValue?: string;

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.offsetTime
			? (this._localValue = this.value as string)
			: (this._localValue = this.#getLocal(this.value as string));
	}

	#getUTC(timeLocal: string) {
		const date = new Date(timeLocal);
		const isoDate = date.toISOString();
		return `${isoDate.substring(0, 10)}T${isoDate.substring(11, 19)}Z`;
	}

	#getLocal(timeUTC: string) {
		const local = new Date(timeUTC);
		const localString = `${local.getFullYear()}-${('0' + (local.getMonth() + 1)).slice(-2)}-${(
			'0' + local.getDate()
		).slice(-2)}T${('0' + local.getHours()).slice(-2)}:${('0' + local.getMinutes()).slice(-2)}:${(
			'0' + local.getSeconds()
		).slice(-2)}Z`;
		return localString;
	}

	#onDatetimeChange(e: UUIInputEvent) {
		e.stopPropagation();
		const pickedTime = e.target.value as string;
		this.offsetTime ? (this.value = pickedTime) : (this.value = this.#getUTC(pickedTime));

		this._localValue = pickedTime;
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`<uui-input
				id="datetime"
				@change="${this.#onDatetimeChange}"
				label="Pick a date or time"
				.type="${this.type}"
				min="${ifDefined(this.min)}"
				max="${ifDefined(this.max)}"
				.step="${this.step}"
				.value="${this._localValue?.replace('Z', '')}"></uui-input>
			<br /><br />
			UTC: ${this.value}<br />
			Local ${this._localValue}<br />
			offset: ${this.offsetTime}`;
	}
}

export default UmbDateInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-input': UmbDateInputElement;
	}
}
