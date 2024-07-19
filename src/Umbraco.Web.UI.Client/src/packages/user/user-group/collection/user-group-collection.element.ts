import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

import './user-group-collection-header.element.js';

const elementName = 'umb-user-group-collection';
@customElement(elementName)
export class UmbUserGroupCollectionElement extends UmbCollectionDefaultElement {
	protected override renderToolbar() {
		return html`<umb-user-group-collection-header slot="header"></umb-user-group-collection-header> `;
	}
}

export { UmbUserGroupCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserGroupCollectionElement;
	}
}
