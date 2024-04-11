import type { UmbDocumentItemModel } from '../repository/index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'umb-document-search-result-item';
@customElement(elementName)
export class UmbDocumentSearchResultItemElement extends UmbLitElement {
	@property({ attribute: false })
	item?: UmbDocumentItemModel;

	render() {
		return html`HELLO WORLD ${this.item?.unique}`;
	}
}

export { UmbDocumentSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentSearchResultItemElement;
	}
}
