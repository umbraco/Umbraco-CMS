import { UmbPropertyValueChangeEvent } from '../../core/property-editor/index.js';
import type { UmbPropertyEditorConfigCollection } from '../../core/property-editor/index.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * This property editor allows the user to pick a date, datetime-local, or time.
 * It uses raw datetime strings back and forth, and therefore it has no knowledge
 * of timezones. It uses a regular HTML input element underneath, which supports the known
 * definitions of "date", "datetime-local", and "time".
 *
 * The underlying input element reports the value differently depending on the type configuration. Here
 * are some examples from the change event:
 *
 * date: 2024-05-03
 * datetime-local: 2024-05-03T10:44
 * time: 10:44
 *
 * These values are approximately similar to what Umbraco expects for the Umbraco.DateTime
 * data editor with one exception: the "T" character in "datetime-local". To be backwards compatible, we are
 * replacing the T character with a whitespace, which also happens to work just fine
 * with the "datetime-local" type.
 *
 * @element umb-property-editor-ui-date-picker
 */
@customElement('umb-property-editor-ui-date-picker')
export class UmbPropertyEditorUIDatePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@state()
	private _inputType: UmbInputDateElement['type'] = 'datetime-local';

	@state()
	private _min?: string;

	@state()
	private _max?: string;

	@state()
	private _step?: number;

	@property()
	set value(value: string | undefined) {
		// Replace the potential time demoninator 'T' with a whitespace for backwards compatibility
		this.#value = value?.replace('T', ' ');
		console.log('got value', value, 'translated to', this.#value);
	}
	get value() {
		return this.#value;
	}
	#value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		// Format string prevalue/config
		const format = config.getValueByAlias<string>('format');
		const hasTime = format?.includes('H') || format?.includes('m');
		this._inputType = hasTime ? 'datetime-local' : 'date';

		// Based on the type of format string change the UUI-input type
		const timeFormatPattern = /^h{1,2}:m{1,2}(:s{1,2})?\s?a?$/gim;
		if (format?.toLowerCase().match(timeFormatPattern)) {
			this._inputType = 'time';
		}

		this._min = config.getValueByAlias('min');
		this._max = config.getValueByAlias('max');
		this._step = config.getValueByAlias('step');

		// If the inputType is only 'date' we need to make sure the value doesn't have a time
		if (this._inputType === 'date' && this.value?.includes(' ')) {
			this.value = this.value.split(' ')[0];
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputDateElement }) {
		this.value = event.target.value.toString();
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-date
				value="${ifDefined(this.value)}"
				min=${ifDefined(this._min)}
				max=${ifDefined(this._max)}
				step=${ifDefined(this._step)}
				type=${this._inputType}
				@change=${this.#onChange}>
			</umb-input-date>
		`;
	}
}

export default UmbPropertyEditorUIDatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-picker': UmbPropertyEditorUIDatePickerElement;
	}
}
