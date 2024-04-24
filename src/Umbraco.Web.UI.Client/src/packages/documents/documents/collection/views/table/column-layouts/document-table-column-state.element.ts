import type { UmbDocumentCollectionItemModel } from '../../../types.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-document-table-column-state')
export class UmbDocumentTableColumnStateElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbDocumentCollectionItemModel;

	render() {
		switch (this.value.state) {
			case 'Published':
				return html`<uui-tag color="positive" look="secondary">${this.localize.term('content_published')}</uui-tag>`;
			case 'PublishedPendingChanges':
				return html`<uui-tag color="warning" look="secondary">${this.localize.term('content_publishedPendingChanges')}</uui-tag>`;
			case 'Draft':
				return html`<uui-tag color="default" look="secondary">${this.localize.term('content_unpublished')}</uui-tag>`;
			case 'NotCreated':
				return html`<uui-tag color="danger" look="secondary">${this.localize.term('content_notCreated')}</uui-tag>`;
			default:
				return html`<uui-tag color="danger" look="secondary">${fromCamelCase(this.value.state)}</uui-tag>`;
		}
	}
}

export default UmbDocumentTableColumnStateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-state': UmbDocumentTableColumnStateElement;
	}
}
