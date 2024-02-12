import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

import './document-collection-toolbar.element.js';

@customElement('umb-document-collection')
export class UmbDocumentCollectionElement extends UmbCollectionDefaultElement {
	protected renderToolbar() {
		return html`<umb-document-collection-toolbar slot="header"></umb-document-collection-toolbar>`;
	}

	// TODO: [LK] How to wire up the `bulkActionPermissions` config with the `entityBulkAction` extension type matches?

	protected renderSelectionActions() {
		return html`<umb-collection-selection-actions slot="footer-info"></umb-collection-selection-actions>`;
	}
}

export default UmbDocumentCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-collection': UmbDocumentCollectionElement;
	}
}
