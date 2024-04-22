import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-select
 */
@customElement('umb-property-editor-ui-select')
export class UmbPropertyEditorUISelectElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string = '';

	@state()
	private _list: Array<Option> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const listData = config.getValueByAlias<string[]>('items');
		this._list = listData?.map((option) => ({ value: option, name: option, selected: option === this.value })) ?? [];
	}

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<uui-select .options=${this._list} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUISelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-select': UmbPropertyEditorUISelectElement;
	}
}
