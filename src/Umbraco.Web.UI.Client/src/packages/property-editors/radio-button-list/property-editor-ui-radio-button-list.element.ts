import type { UmbInputRadioButtonListElement } from '../../core/components/input-radio-button-list/input-radio-button-list.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../core/components/input-radio-button-list/input-radio-button-list.element.js';

/**
 * @element umb-property-editor-ui-radio-button-list
 */
@customElement('umb-property-editor-ui-radio-button-list')
export class UmbPropertyEditorUIRadioButtonListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string = '';

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const listData: string[] | undefined = config?.getValueByAlias('items');
		this._list = listData?.map((item) => ({ label: item, value: item })) ?? [];
	}

	@state()
	private _list: UmbInputRadioButtonListElement['list'] = [];

	#onChange(event: CustomEvent) {
		const element = event.target as UmbInputRadioButtonListElement;
		this.value = element.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-radio-button-list
			.list=${this._list}
			.value=${this.value ?? ''}
			@change=${this.#onChange}></umb-input-radio-button-list>`;
	}
}

export default UmbPropertyEditorUIRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-radio-button-list': UmbPropertyEditorUIRadioButtonListElement;
	}
}
