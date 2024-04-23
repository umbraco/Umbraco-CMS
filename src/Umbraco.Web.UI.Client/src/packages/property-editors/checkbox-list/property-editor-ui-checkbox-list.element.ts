import type { UmbInputCheckboxListElement } from './input-checkbox-list/input-checkbox-list.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import './input-checkbox-list/input-checkbox-list.element.js';

/**
 * @element umb-property-editor-ui-checkbox-list
 */
@customElement('umb-property-editor-ui-checkbox-list')
export class UmbPropertyEditorUICheckboxListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#value: Array<string> = [];
	@property({ type: Array })
	public set value(value: Array<string>) {
		this.#value = value ?? [];
	}
	public get value(): Array<string> {
		return this.#value;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const listData: string[] | undefined = config?.getValueByAlias('items');
		this._list = listData?.map((item) => ({ label: item, value: item, checked: this.#value.includes(item) })) ?? [];
	}

	@state()
	private _list: UmbInputCheckboxListElement['list'] = [];

	#onChange(event: CustomEvent) {
		const element = event.target as UmbInputCheckboxListElement;
		this.value = element.selection;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-checkbox-list
				.list=${this._list}
				.selection=${this.#value}
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
