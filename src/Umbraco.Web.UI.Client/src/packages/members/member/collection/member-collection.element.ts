import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

import './member-collection-header.element.js';

@customElement('umb-member-collection')
export class UmbMemberCollectionElement extends UmbCollectionDefaultElement {
	protected override renderToolbar() {
		return html`<umb-member-collection-header slot="header"></umb-member-collection-header>`;
	}
}

/** @deprecated Should be exported as `element` only; to be removed in Umbraco 17. */
export default UmbMemberCollectionElement;

export { UmbMemberCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-collection': UmbMemberCollectionElement;
	}
}
