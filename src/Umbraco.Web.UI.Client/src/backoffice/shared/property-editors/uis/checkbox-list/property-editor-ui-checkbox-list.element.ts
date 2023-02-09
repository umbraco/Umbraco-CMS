import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import '../../../components/input-checkbox-list/input-checkbox-list.element';
import { UmbInputCheckboxListElement } from '../../../components/input-checkbox-list/input-checkbox-list.element';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DataTypePropertyData } from '@umbraco-cms/models';

/**
 * @element umb-property-editor-ui-checkbox-list
 */
@customElement('umb-property-editor-ui-checkbox-list')
export class UmbPropertyEditorUICheckboxListElement extends UmbLitElement {
	static styles = [UUITextStyles];

	private _value: Array<string> = [];
	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
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
		this.value = (event.target as UmbInputCheckboxListElement).selectedKeys;
		this.dispatchEvent(new CustomEvent('property-value-change'));
		console.log(this._value);
	}

	render() {
		return html`<umb-input-checkbox-list
			@change="${this._onChange}"
			.selectedKeys="${this._value}"
			.list="${this._list}"></umb-input-checkbox-list>`;
	}
}

export default UmbPropertyEditorUICheckboxListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-checkbox-list': UmbPropertyEditorUICheckboxListElement;
	}
}
