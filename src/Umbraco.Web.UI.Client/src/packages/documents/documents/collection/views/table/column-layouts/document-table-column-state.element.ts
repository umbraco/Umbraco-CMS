import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-document-table-column-state')
export class UmbDocumentTableColumnStateElement extends UmbLitElement implements UmbTableColumnLayoutElement {
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
			this.#item.setData(value.item);
		}
	}

	@state()
	_state = '';

	#item = new UmbDocumentItemDataResolver(this);

	constructor() {
		super();
		this.#item.observe(this.#item.state, (state) => (this._state = state));
	}

	override render() {
		switch (this._state) {
			case 'Published':
				return html`<uui-tag color="positive" look="secondary">${this.localize.term('content_published')}</uui-tag>`;
			case 'PublishedPendingChanges':
				return html`<uui-tag color="warning" look="secondary"
					>${this.localize.term('content_publishedPendingChanges')}</uui-tag
				>`;
			case 'Draft':
				return html`<uui-tag color="default" look="secondary">${this.localize.term('content_unpublished')}</uui-tag>`;
			case 'NotCreated':
				return html`<uui-tag color="danger" look="secondary">${this.localize.term('content_notCreated')}</uui-tag>`;
			default:
				return html`<uui-tag color="danger" look="secondary">${fromCamelCase(this.value.item.state)}</uui-tag>`;
		}
	}
}

export default UmbDocumentTableColumnStateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-state': UmbDocumentTableColumnStateElement;
	}
}
