import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-has-collection-entity-sign')
export class UmbDocumentHasCollectionEntitySignElement extends UmbLitElement {
	override render() {
		return html`<umb-icon name="icon-grid" title="Collection"></umb-icon>`;
	}
}

export { UmbDocumentHasCollectionEntitySignElement as element };
