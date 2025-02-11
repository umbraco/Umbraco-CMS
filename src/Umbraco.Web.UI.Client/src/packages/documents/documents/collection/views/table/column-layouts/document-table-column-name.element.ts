import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-document-table-column-name')
export class UmbDocumentTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	#value!: UmbEditableDocumentCollectionItemModel;
	@property({ attribute: false })
	public get value(): UmbEditableDocumentCollectionItemModel {
		return this.#value;
	}
	public set value(value: UmbEditableDocumentCollectionItemModel) {
		this.#value = value;

		if (value.item) {
			this.#item.setItem(value.item);
		}
	}

	@state()
	_name = '';

	#item = new UmbDocumentItemDataResolver(this);

	constructor() {
		super();
		this.#item.observe(this.#item.name, (name) => (this._name = name || ''));
	}

	override render() {
		if (!this.value) return nothing;
		return html` <uui-button compact href=${this.value.editPath} label=${this._name}></uui-button> `;
	}

	static override styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbDocumentTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-name': UmbDocumentTableColumnNameElement;
	}
}
