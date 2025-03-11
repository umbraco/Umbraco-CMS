import type {
	UmbCheckboxListItem,
	UmbInputCheckboxListElement,
} from './components/input-checkbox-list/input-checkbox-list.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

import './components/input-checkbox-list/input-checkbox-list.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-checkbox-list
 */
@customElement('umb-property-editor-ui-checkbox-list')
export class UmbPropertyEditorUICheckboxListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#selection: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string> | string | undefined) {
		this.#selection = Array.isArray(value) ? value : value ? [value] : [];
	}
	public get value(): Array<string> | undefined {
		return this.#selection;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length) {
			this._list =
				typeof items[0] === 'string'
					? items.map((item) => ({ label: item, value: item, checked: this.#selection.includes(item) }))
					: items.map((item) => ({
							label: item.name,
							value: item.value,
							checked: this.#selection.includes(item.value),
						}));

			// If selection includes a value that is not in the list, add it to the list
			this.#selection.forEach((value) => {
				if (!this._list.find((item) => item.value === value)) {
					this._list.push({ label: value, value, checked: true, invalid: true });
				}
			});
		}
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _list: Array<UmbCheckboxListItem> = [];

	#onChange(event: CustomEvent & { target: UmbInputCheckboxListElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-checkbox-list
				.list=${this._list}
				.selection=${this.#selection}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-checkbox-list>
		`;
	}
}

export default UmbPropertyEditorUICheckboxListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-checkbox-list': UmbPropertyEditorUICheckboxListElement;
	}
}
