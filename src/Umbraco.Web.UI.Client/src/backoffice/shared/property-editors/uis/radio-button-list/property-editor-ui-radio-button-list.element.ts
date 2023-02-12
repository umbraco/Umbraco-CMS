import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import '../../../components/input-radio-button-list/input-radio-button-list.element';
import type { UmbInputRadioButtonListElement } from '../../../components/input-radio-button-list/input-radio-button-list.element';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DataTypePropertyData } from '@umbraco-cms/models';

/**
 * @element umb-property-editor-ui-radio-button-list
 */
@customElement('umb-property-editor-ui-radio-button-list')
export class UmbPropertyEditorUIRadioButtonListElement extends UmbLitElement {
	static styles = [UUITextStyles];

	private _value = '';
	@property({ type: String })
	public get value(): string {
		return this._value;
	}
	public set value(value: string) {
		this._value = value || '';
	}

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyData>) {
		const listData = config.find((x) => x.alias === 'itemList');

		if (!listData) return;
		this._list = listData.value;
	}

	@state()
	private _list: [] = [];

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputRadioButtonListElement).selectedKey;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-radio-button-list
			@change="${this._onChange}"
			.selectedKeys="${this._value}"
			.list="${this._list}"></umb-input-radio-button-list>`;
	}
}

export default UmbPropertyEditorUIRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-radio-button-list': UmbPropertyEditorUIRadioButtonListElement;
	}
}
