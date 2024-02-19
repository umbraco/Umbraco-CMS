import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-dropdown
 */
@customElement('umb-property-editor-ui-dropdown')
export class UmbPropertyEditorUIDropdownElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#value = '';
	@property({ type: String })
	public get value(): string {
		return this.#value;
	}
	public set value(value: string | undefined) {
		this.#value = value?.trim() || '';
	}

	@state()
	_multiple?: boolean;

	@state()
	private _list: Array<Option> = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this._multiple = config?.getValueByAlias('multiple');

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

		this._list = sortedItems.map((x) => ({ value: x.value, name: x.value, selected: x.value === this.value }));
	}

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-dropdown-list
			@change="${this.#onChange}"
			?multiple=${this._multiple}
			.options="${this._list}"></umb-input-dropdown-list>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dropdown': UmbPropertyEditorUIDropdownElement;
	}
}
