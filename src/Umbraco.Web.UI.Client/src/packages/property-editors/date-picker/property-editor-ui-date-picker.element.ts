import { UmbPropertyValueChangeEvent } from '../../core/property-editor/index.js';
import type { UmbPropertyEditorConfigCollection } from '../../core/property-editor/index.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
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
		if (value) {
			// NOTE: If the `value` contains a space, then it doesn't contain the timezone, so may not be parsed as UTC. [LK]
			const datetime = !value.includes(' ') ? value : value + ' +00';
			this.#value = new Date(datetime).toJSON();
		}
	}
	get value() {
		return this.#value;
	}
	#value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		const oldVal = this._inputType;

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

		this.requestUpdate('_inputType', oldVal);
	}

	#onChange(event: CustomEvent & { target: HTMLInputElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-date
				value="${ifDefined(this.value)}"
				.min=${this._min}
				.max=${this._max}
				.step=${this._step}
				.type=${this._inputType}
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
