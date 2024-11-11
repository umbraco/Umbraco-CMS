import { html, customElement, property, state, query } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectElement, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	type UmbPropertyEditorUiElement,
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-value-type
 */

@customElement('umb-property-editor-ui-value-type')
export class UmbPropertyEditorUIValueTypeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: string | undefined = undefined;
	@property()
	public get value(): string | undefined {
		return this._value;
	}
	public set value(value: string | undefined) {
		this._value = value;

		const selected = this._options.filter((option) => {
			if (this.selectEl && option.value === this.value) this.selectEl.value = this.value;
			return (option.selected = option.value === this.value);
		});
		if (selected.length === 0) {
			this._options[0].selected = true;
		}
	}

	@query('uui-select')
	selectEl?: UUISelectElement;

	@state()
	private _options: Array<Option> = [
		{ name: 'String', value: 'STRING' },
		{ name: 'Decimal', value: 'DECIMAL' },
		{ name: 'Date/Time', value: 'DATETIME' },
		{ name: 'Time', value: 'TIME' },
		{ name: 'Integer', value: 'INT' },
		{ name: 'Big Integer', value: 'BIGINT' },
		{ name: 'Long String', value: 'TEXT' },
	];

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(e: UUISelectEvent) {
		this.value = e.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<uui-select
			label="Select a value type"
			.options="${this._options}"
			@change="${this.#onChange}"></uui-select>`;
	}
}

export default UmbPropertyEditorUIValueTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-value-type': UmbPropertyEditorUIValueTypeElement;
	}
}
