import { updateItemsSelectedState } from '../utils/property-editor-ui-state-manager.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-select
 */
@customElement('umb-property-editor-ui-select')
export class UmbPropertyEditorUISelectElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: string = '';

	// Update the selected state of existing options when value changes
	@property()
	public set value(newValue: string | undefined) {
		this._value = newValue || '';
		this.#updateSelectedState();
	}
	public get value(): string {
		return this._value;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length > 0) {
			this._options =
				typeof items[0] === 'string'
					? items.map((item) => ({ name: item, value: item, selected: item === this._value }))
					: items.map((item) => ({ name: item.name, value: item.value, selected: item.value === this._value }));
		}
	}

	@state()
	private _options: Array<Option> = [];

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Updates the selected state of all options based on current value.
	 * This fixes the issue where UI doesn't update when values are set programmatically.
	 */
	#updateSelectedState() {
		// Only update if we have options loaded
		if (this._options.length > 0) {
			// Update state only if changes are needed
			const updatedOptions = updateItemsSelectedState(this._options, [this._value], 'selected');
			if (updatedOptions !== this._options) {
				this._options = updatedOptions;
			}
		}
	}

	override render() {
		return html`<uui-select .options=${this._options} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUISelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-select': UmbPropertyEditorUISelectElement;
	}
}
