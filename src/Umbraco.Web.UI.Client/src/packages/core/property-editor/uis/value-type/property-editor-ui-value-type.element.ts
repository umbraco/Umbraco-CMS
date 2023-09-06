import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-value-type
 */

@customElement('umb-property-editor-ui-value-type')
export class UmbPropertyEditorUIValueTypeElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	private _value: string | undefined = undefined;
	@property()
	public get value(): string | undefined {
		return this._value;
	}
	public set value(value: string | undefined) {
		this._value = value;

		const selected = this._options.filter((option) => {
			return (option.selected = option.value === this.value);
		});
		if (selected.length === 0) {
			this._options[0].selected = true;
		}
	}

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
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-select
			label="Select a value type"
			.options="${this._options}"
			@change="${this.#onChange}"></uui-select>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIValueTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-value-type': UmbPropertyEditorUIValueTypeElement;
	}
}
