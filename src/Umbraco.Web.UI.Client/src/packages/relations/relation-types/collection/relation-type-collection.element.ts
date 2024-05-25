import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';

const elementName = 'umb-relation-type-collection';
@customElement(elementName)
export class UmbRelationTypeCollectionElement extends UmbCollectionDefaultElement {
	// NOTE: Returns empty toolbar, so to remove the header padding.
	protected renderToolbar() {
		return html``;
	}
}

export default UmbRelationTypeCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbRelationTypeCollectionElement;
	}
}
