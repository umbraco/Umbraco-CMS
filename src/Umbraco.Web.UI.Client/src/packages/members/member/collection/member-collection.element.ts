import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

// import './member-collection-header.element.js';

@customElement('umb-member-collection')
export class UmbMemberCollectionElement extends UmbCollectionDefaultElement {
	// protected renderToolbar() {
	// 	return html`<umb-member-collection-header slot="header"></umb-member-collection-header> `;
	// }
}

export default UmbMemberCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-collection': UmbMemberCollectionElement;
	}
}
