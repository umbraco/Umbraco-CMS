import { UmbDocumentCollectionItemDataResolver } from '../../../document-collection-item-data-resolver.js';
import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-document-table-column-system-value')
export class UmbDocumentTableColumnSystemValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentCollectionItemDataResolver(this);

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbEditableDocumentCollectionItemModel) {
		this.#value = value;
		if (value.item) {
			this.#resolver.setData(value.item);
		}
	}
	public get value(): UmbEditableDocumentCollectionItemModel {
		return this.#value;
	}
	#value!: UmbEditableDocumentCollectionItemModel;

	override render() {
		if (!this.value) return nothing;
		const value = this.#resolver.getSystemValue(this.column.alias);
		return when(
			this.column.labelTemplate,
			() => html`<umb-ufm-render inline .markdown=${this.column.labelTemplate} .value=${{ value }}></umb-ufm-render>`,
			() => html`${value}`,
		);
	}
}

export default UmbDocumentTableColumnSystemValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-system-value': UmbDocumentTableColumnSystemValueElement;
	}
}
