import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

@customElement('umb-document-collection')
export class UmbDocumentCollectionElement extends UmbCollectionDefaultElement {
	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
		`;
	}
}

/** @deprecated Should be exported as `element` only; to be removed in Umbraco 17. */
export default UmbDocumentCollectionElement;

export { UmbDocumentCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-collection': UmbDocumentCollectionElement;
	}
}
