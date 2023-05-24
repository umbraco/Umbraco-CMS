import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import '../../../components/input-radio-button-list/input-radio-button-list.element.js';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import type { UmbInputRadioButtonListElement } from '../../../components/input-radio-button-list/input-radio-button-list.element.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-radio-button-list
 */
@customElement('umb-property-editor-ui-radio-button-list')
export class UmbPropertyEditorUIRadioButtonListElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	#value = '';
	@property({ type: String })
	public get value(): string {
		return this.#value;
	}
	public set value(value: string) {
		this.#value = value || '';
	}

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		const listData: Record<number, { value: string; sortOrder: number }> | undefined = config.getValueByAlias('items');

		if (!listData) return;

		// formatting the items in the dictionary into an array
		const sortedItems = [];
		const values = Object.values<{ value: string; sortOrder: number }>(listData);
		const keys = Object.keys(listData);
		for (let i = 0; i < values.length; i++) {
			sortedItems.push({ key: keys[i], sortOrder: values[i].sortOrder, value: values[i].value });
		}

		// ensure the items are sorted by the provided sort order
		sortedItems.sort((a, b) => {
			return a.sortOrder > b.sortOrder ? 1 : b.sortOrder > a.sortOrder ? -1 : 0;
		});

		this._list = sortedItems;
	}

	@state()
	private _list: Array<{ key: string; sortOrder: number; value: string }> = [];

	#onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputRadioButtonListElement).selected;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-radio-button-list
			@change="${this.#onChange}"
			.selectedKey="${this.#value}"
			.list="${this._list}"></umb-input-radio-button-list>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-radio-button-list': UmbPropertyEditorUIRadioButtonListElement;
	}
}
