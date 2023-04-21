import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-date-input')
export class UmbDateInputElement extends FormControlMixin(UmbLitElement) {
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

	@property({ type: String })
	min?: string;

	@property({ type: String })
	max?: string;

	@property({ type: Number })
	step?: number;

	@property({ type: Boolean })
	offsetTime = false; //TODO

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.value = this.datetime;
	}

	#onDatetimeChange(e: UUIInputEvent) {
		e.stopPropagation();

		const pickedTime = e.target.value as string;
		this.datetime = pickedTime;

		// Set property editor value to UTC
		const date = new Date(pickedTime);
		const isoDate = date.toISOString();
		this.value = `${isoDate.substring(0, 10)} ${isoDate.substring(11, 19)}`;

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
			.value="${this.datetime}"></uui-input>`;
	}
}

export default UmbDateInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-input': UmbDateInputElement;
	}
}
