import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

import './user-collection-header.element.js';

@customElement('umb-user-collection')
export class UmbUserCollectionElement extends UmbCollectionDefaultElement {
	protected renderToolbar() {
		return html`<umb-user-collection-header slot="header"></umb-user-collection-header> `;
	}
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection': UmbUserCollectionElement;
	}
}
