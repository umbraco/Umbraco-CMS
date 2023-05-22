import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type { UUISelectEvent } from '@umbraco-ui/uui';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-ui-value-type
 */

@customElement('umb-property-editor-ui-value-type')
export class UmbPropertyEditorUIValueTypeElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

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

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		const index = this._options.findIndex((option) => option.value === this.value);
		index > 0 ? (this._options[index].selected = true) : (this._options[0].selected = true);
	}

	@property({ type: Array, attribute: false })
	public config?: UmbDataTypePropertyCollection;

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

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIValueTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-value-type': UmbPropertyEditorUIValueTypeElement;
	}
}
