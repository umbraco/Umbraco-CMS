import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-collection-view-order-by
 */
@customElement('umb-property-editor-ui-collection-view-order-by')
export class UmbPropertyEditorUICollectionViewOrderByElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	private _value = '';
	@property()
	public set value(v: string) {
		this._value = v;
		this._options = this._options.map((option) => (option.value === v ? { ...option, selected: true } : option));
	}
	public get value() {
		return this._value;
	}

	@state()
	_options: Array<Option> = [
		{ value: 'name', name: 'Name' },
		{ value: 'updateDate', name: 'Last edited' },
		{ value: 'owner', name: 'Created by' },
	];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(e: UUISelectEvent) {
		this.value = e.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<uui-select label="select" .options=${this._options} @change=${this.#onChange}></uui-select>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUICollectionViewOrderByElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-order-by': UmbPropertyEditorUICollectionViewOrderByElement;
	}
}
