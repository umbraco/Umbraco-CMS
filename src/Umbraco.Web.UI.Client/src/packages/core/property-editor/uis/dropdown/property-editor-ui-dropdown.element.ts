import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
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
	private _list: Array<Option> = [];

	@state()
	private _multiple?: boolean;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const listData: string[] | undefined = config?.getValueByAlias('items');
		this._list = listData?.map((x) => ({ value: x, name: x, selected: this.#selection.includes(x) })) ?? [];
		this._multiple = config?.getValueByAlias('multiple');
	}

	#onChange(event: UUISelectEvent) {
		const value = event.target.value as string;
		this.value = value ? [value] : [];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-dropdown-list
			@change="${this.#onChange}"
			?multiple=${this._multiple}
			.options="${this._list}"></umb-input-dropdown-list>`;
	}
}

export default UmbPropertyEditorUIDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-dropdown': UmbPropertyEditorUIDropdownElement;
	}
}
