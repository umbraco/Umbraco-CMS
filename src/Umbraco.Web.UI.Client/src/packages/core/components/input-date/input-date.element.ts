import { html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * This element passes a datetime string to a regular HTML input element.
 *
 * @remark Be aware that you cannot include a time demonination, i.e. "10:44:00" if you
 * set the input type of this element to "date". If you do, the browser will not show
 * the value at all.
 *
 * @element umb-input-date
 */
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
	min?: string;

	@property({ type: String })
	max?: string;

	@property({ type: Number })
	step?: number;

	#onChange(event: UUIInputEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<uui-input
			id="datetime"
			.label=${this.localize.term('placeholders_enterdate')}
			.min=${this.min}
			.max=${this.max}
			.step=${this.step}
			.type=${this.type}
			value=${ifDefined(this.value)}
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
