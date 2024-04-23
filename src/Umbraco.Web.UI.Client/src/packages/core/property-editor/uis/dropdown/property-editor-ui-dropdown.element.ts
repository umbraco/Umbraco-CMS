import { css, customElement, html, map, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-dropdown
 */
@customElement('umb-property-editor-ui-dropdown')
export class UmbPropertyEditorUIDropdownElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#selection: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string> | string | undefined) {
		this.#selection = Array.isArray(value) ? value : value ? [value] : [];
	}
	public get value(): Array<string> | undefined {
		return this.#selection;
	}

	@state()
	private _items: Array<Option> = [];

	@state()
	private _multiple: boolean = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias<string[]>('items');
		this._items = items?.map((item) => ({ value: item, name: item, selected: this.#selection.includes(item) })) ?? [];

		this._multiple = config?.getValueByAlias<boolean>('multiple') ?? false;
	}

	#onChange(event: UUISelectEvent) {
		const value = event.target.value as string;
		this.#setValue(value ? [value] : []);
	}

	#onChangeMulitple(event: Event & { target: HTMLSelectElement }) {
		const selected = event.target.selectedOptions;
		const value = selected ? Array.from(selected).map((option) => option.value) : [];
		this.#setValue(value);
	}

	#setValue(value: Array<string> | string | null | undefined) {
		if (!value) return;
		this.value = value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return this._multiple ? this.#renderDropdownMultiple() : this.#renderDropdownSingle();
	}

	#renderDropdownMultiple() {
		return html`
			<select id="native" multiple @change=${this.#onChangeMulitple}>
				${map(
					this._items,
					(item) => html`<option value=${item.value} ?selected=${item.selected}>${item.name}</option>`,
				)}
			</select>
		`;
	}

	#renderDropdownSingle() {
		return html`<umb-input-dropdown-list .options=${this._items} @change=${this.#onChange}></umb-input-dropdown-list>`;
	}

	static styles = [
		UUISelectElement.styles,
		css`
			#native {
				height: auto;
			}
		`,
	];
}

export default UmbPropertyEditorUIDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dropdown': UmbPropertyEditorUIDropdownElement;
	}
}
